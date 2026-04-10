using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

public class Fat12BootSector
{
    public ushort BytesPerSector { get; set; } = 512;
    public byte SectorsPerCluster { get; set; } = 1;
    public ushort ReservedSectors { get; set; } = 1;
    public byte NumberOfFats { get; set; } = 2;
    public ushort RootEntries { get; set; } = 224;
    public ushort TotalSectors16 { get; set; }
    public uint TotalSectors32 { get; set; }
    public ushort SectorsPerFat { get; set; }
    public ushort SectorsPerTrack { get; set; }
    public ushort NumberOfHeads { get; set; }
    public string VolumeLabel { get; set; } = "";
    
    public bool IsValid => BytesPerSector == 512 && NumberOfFats > 0 && RootEntries > 0;
    public bool IsValidFat12() => IsValid && SectorsPerFat > 0 && TotalSectors16 > 0;
    public int FirstDataSector => ReservedSectors + (NumberOfFats * SectorsPerFat) + ((RootEntries * 32 + BytesPerSector - 1) / BytesPerSector);

    public static Fat12BootSector Parse(byte[] data)
    {
        if (data.Length < 512)
            throw new ArgumentException("Boot sector data too short");

        var bootSector = new Fat12BootSector();
        
        bootSector.BytesPerSector = BitConverter.ToUInt16(data, 11);
        bootSector.SectorsPerCluster = data[13];
        bootSector.ReservedSectors = BitConverter.ToUInt16(data, 14);
        bootSector.NumberOfFats = data[16];
        bootSector.RootEntries = BitConverter.ToUInt16(data, 17);
        bootSector.TotalSectors16 = BitConverter.ToUInt16(data, 19);
        bootSector.SectorsPerFat = BitConverter.ToUInt16(data, 22);
        bootSector.SectorsPerTrack = BitConverter.ToUInt16(data, 24);
        bootSector.NumberOfHeads = BitConverter.ToUInt16(data, 26);
        bootSector.TotalSectors32 = BitConverter.ToUInt32(data, 32);
        
        if (data.Length >= 54)
        {
            bootSector.VolumeLabel = Encoding.ASCII.GetString(data, 43, 11).Trim();
        }

        return bootSector;
    }
}

public class Fat12DirectoryEntry
{
    public string Name { get; set; } = "";
    public string Extension { get; set; } = "";
    public byte Attributes { get; set; }
    public ushort WriteTime { get; set; }
    public ushort WriteDate { get; set; }
    public ushort FirstCluster { get; set; }
    public uint FileSize { get; set; }
    
    public bool IsDeleted => Name.StartsWith("\x00") || Name.StartsWith("\xE5");
    public bool IsEndOfDirectory => Name.StartsWith("\x00");
    public bool IsDirectory => (Attributes & 0x10) != 0;
    public bool IsReadOnly => (Attributes & 0x01) != 0;
    public bool IsHidden => (Attributes & 0x02) != 0;
    public bool IsVolumeLabel => (Attributes & 0x08) != 0;

    public string GetFileName()
    {
        return Name.Trim();
    }

    public string GetExtension()
    {
        return Extension.Trim();
    }

    public DateTime GetModifiedDate()
    {
        return ConvertDosDateTime(WriteDate, WriteTime);
    }

    private static DateTime ConvertDosDateTime(ushort date, ushort time)
    {
        if (date == 0) return DateTime.MinValue;
        
        try
        {
            var year = 1980 + ((date >> 9) & 0x7F);
            var month = (date >> 5) & 0x0F;
            var day = date & 0x1F;
            
            var hour = (time >> 11) & 0x1F;
            var minute = (time >> 5) & 0x3F;
            var second = (time & 0x1F) * 2;
            
            return new DateTime(year, month, day, hour, minute, second);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public static Fat12DirectoryEntry Parse(byte[] data)
    {
        if (data.Length < 32)
            throw new ArgumentException("Directory entry data too short");

        var entry = new Fat12DirectoryEntry();
        
        entry.Name = Encoding.ASCII.GetString(data, 0, 8).Trim();
        entry.Extension = Encoding.ASCII.GetString(data, 8, 3).Trim();
        entry.Attributes = data[11];
        entry.WriteTime = BitConverter.ToUInt16(data, 22);
        entry.WriteDate = BitConverter.ToUInt16(data, 24);
        entry.FirstCluster = BitConverter.ToUInt16(data, 26);
        entry.FileSize = BitConverter.ToUInt32(data, 28);

        return entry;
    }
}

public class Fat12FileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private Fat12BootSector _bootSector = new();
    private byte[] _fatTable = Array.Empty<byte>();
    private readonly List<Fat12DirectoryEntry> _rootDirectory = new();
    
    // FAT12 constants
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB safety limit
    private const int MaxClusterChainLength = 4000; // Prevent infinite loops

    public IDiskContainer DiskContainer => _diskContainer;
    public bool IsFormatted => CheckIfFormatted();
    
    public Fat12FileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer ?? throw new ArgumentNullException(nameof(diskContainer));
        LoadFileSystem();
    }

    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot format read-only disk");

        // Create default boot sector
        _bootSector = CreateDefaultBootSector();
        
        // Write boot sector
        WriteBootSector();
        
        // Initialize FAT
        InitializeFat();
        
        // Initialize root directory  
        InitializeRootDirectory();
    }

    private void WriteBootSector()
    {
        var bootData = new byte[512];
        
        // FAT12 Boot Sector structure
        bootData[0] = 0xEB; // Jump instruction
        bootData[1] = 0x3C;
        bootData[2] = 0x90;
        
        // OEM Name
        Array.Copy(System.Text.Encoding.ASCII.GetBytes("LEGACY89"), 0, bootData, 3, 8);
        
        // BPB (BIOS Parameter Block)
        BitConverter.GetBytes(_bootSector.BytesPerSector).CopyTo(bootData, 11);
        bootData[13] = (byte)_bootSector.SectorsPerCluster;
        BitConverter.GetBytes(_bootSector.ReservedSectors).CopyTo(bootData, 14);
        bootData[16] = (byte)_bootSector.NumberOfFats;
        BitConverter.GetBytes(_bootSector.RootEntries).CopyTo(bootData, 17);
        BitConverter.GetBytes(_bootSector.TotalSectors16).CopyTo(bootData, 19);
        bootData[21] = 0xF0; // Media descriptor
        BitConverter.GetBytes(_bootSector.SectorsPerFat).CopyTo(bootData, 22);
        BitConverter.GetBytes(_bootSector.SectorsPerTrack).CopyTo(bootData, 24);
        BitConverter.GetBytes(_bootSector.NumberOfHeads).CopyTo(bootData, 26);
        
        // Volume label
        Array.Copy(System.Text.Encoding.ASCII.GetBytes(_bootSector.VolumeLabel.PadRight(11)), 0, bootData, 43, 11);
        
        // File system type
        Array.Copy(System.Text.Encoding.ASCII.GetBytes("FAT12   "), 0, bootData, 54, 8);
        
        // Boot signature
        bootData[510] = 0x55;
        bootData[511] = 0xAA;
        
        _diskContainer.WriteSector(0, 0, 1, bootData);
    }

    private void InitializeFat()
    {
        for (int fatCopy = 0; fatCopy < _bootSector.NumberOfFats; fatCopy++)
        {
            for (int sector = 0; sector < _bootSector.SectorsPerFat; sector++)
            {
                var fatData = new byte[512];
                
                if (sector == 0)
                {
                    // First FAT sector - initialize with media descriptor
                    fatData[0] = 0xF0; // Media descriptor
                    fatData[1] = 0xFF;
                    fatData[2] = 0xFF;
                }
                
                var sectorNum = _bootSector.ReservedSectors + (fatCopy * _bootSector.SectorsPerFat) + sector + 1;
                _diskContainer.WriteSector(0, 0, sectorNum, fatData);
            }
        }
    }

    private void InitializeRootDirectory()
    {
        var rootDirSectors = (_bootSector.RootEntries * 32 + 511) / 512;
        var rootDirStart = _bootSector.ReservedSectors + (_bootSector.NumberOfFats * _bootSector.SectorsPerFat);
        
        for (int sector = 0; sector < rootDirSectors; sector++)
        {
            var dirData = new byte[512];
            // Initialize with zeros (empty directory)
            var sectorNum = rootDirStart + sector + 1;
            _diskContainer.WriteSector(0, 0, sectorNum, dirData);
        }
    }

    public IEnumerable<FileEntry> ListFiles()
    {
        var files = new List<FileEntry>();
        
        foreach (var entry in _rootDirectory)
        {
            if (entry.IsDeleted || entry.IsVolumeLabel || entry.IsDirectory)
                continue;
                
            var fileName = entry.GetFileName();
            var extension = entry.GetExtension();
            
            files.Add(new FileEntry(
                fileName,
                extension,
                GetFileMode(entry),
                new HuBasicFileAttributes(false, false, false, false, true, false, false),
                (int)entry.FileSize,
                0,
                0,
                entry.GetModifiedDate(),
                false
            ));
        }
        
        return files;
    }

    public byte[] ReadFile(string fileName, bool allowPartialRead = false)
    {
        var entry = FindFileEntry(fileName);
        if (entry == null)
        {
            throw new FileSystemException($"File not found: {fileName}");
        }

        if (entry.FileSize == 0)
        {
            return Array.Empty<byte>();
        }

        if (entry.FileSize > MaxFileSize)
        {
            throw new FileSystemException($"File too large: {entry.FileSize} bytes (max: {MaxFileSize})");
        }

        try
        {
            return ReadFileData(entry, allowPartialRead);
        }
        catch (Exception ex) when (allowPartialRead)
        {
            Console.WriteLine($"Warning: Partial read due to error: {ex.Message}");
            return ReadFileDataPartial(entry);
        }
    }

    public void WriteFile(string fileName, byte[] data, bool isText = false, ushort loadAddress = 0, ushort execAddress = 0)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot write to read-only disk");
            
        // Simplified implementation for basic functionality
        throw new NotImplementedException("FAT12 file writing - complex implementation required");
    }

    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot delete from read-only disk");
            
        // Simplified implementation for basic functionality
        throw new NotImplementedException("FAT12 file deletion - complex implementation required");
    }

    public BootSector GetBootSector()
    {
        return new BootSector(
            IsBootable: false,
            Label: _bootSector.VolumeLabel.Trim(),
            Extension: "",
            Size: _bootSector.BytesPerSector,
            LoadAddress: 0,
            ExecuteAddress: 0,
            ModifiedDate: DateTime.MinValue,
            StartSector: 0);
    }

    public void WriteBootSector(BootSector bootSector)
    {
        throw new NotImplementedException("FAT12 boot sector writing not yet implemented");
    }

    public HuBasicFileSystemInfo GetFileSystemInfo()
    {
        var totalClusters = CalculateTotalClusters();
        var freeClusters = CalculateFreeClusters();
        
        return new HuBasicFileSystemInfo(
            TotalClusters: totalClusters,
            FreeClusters: freeClusters, 
            ClusterSize: _bootSector.SectorsPerCluster * _bootSector.BytesPerSector,
            SectorSize: _bootSector.BytesPerSector);
    }

    public FileEntry? GetFile(string fileName)
    {
        var entry = _rootDirectory.FirstOrDefault(e => 
            !e.IsDeleted && GetDosFileName(e).Equals(fileName, StringComparison.OrdinalIgnoreCase));
        
        if (entry == null) return null;
        
        return new FileEntry(
            FileName: GetDosFileName(entry),
            Extension: "",
            Mode: HuBasicFileMode.Binary, // FAT12 files are treated as binary
            Attributes: new HuBasicFileAttributes(
                IsDirectory: entry.IsDirectory,
                IsReadOnly: entry.IsReadOnly,
                IsVerify: false,
                IsHidden: entry.IsHidden,
                IsBinary: true,
                IsBasic: false,
                IsAscii: false),
            Size: (int)entry.FileSize,
            LoadAddress: 0,
            ExecuteAddress: 0,
            ModifiedDate: ConvertDosDateTime(entry.WriteDate, entry.WriteTime),
            IsProtected: entry.IsReadOnly);
    }

    public byte[] ReadFile(string fileName)
    {
        return ReadFile(fileName, false);
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        return _rootDirectory.Where(e => !e.IsDeleted && !e.IsDirectory)
            .Select(e => new FileEntry(
                FileName: GetDosFileName(e),
                Extension: "",
                Mode: HuBasicFileMode.Binary,
                Attributes: new HuBasicFileAttributes(
                    IsDirectory: false,
                    IsReadOnly: e.IsReadOnly,
                    IsVerify: false,
                    IsHidden: e.IsHidden,
                    IsBinary: true,
                    IsBasic: false,
                    IsAscii: false),
                Size: (int)e.FileSize,
                LoadAddress: 0,
                ExecuteAddress: 0,
                ModifiedDate: ConvertDosDateTime(e.WriteDate, e.WriteTime),
                IsProtected: e.IsReadOnly));
    }

    private bool CheckIfFormatted()
    {
        try
        {
            LoadBootSector();
            return _bootSector.IsValid;
        }
        catch
        {
            return false;
        }
    }

    private int CalculateTotalClusters()
    {
        if (_bootSector.TotalSectors16 > 0)
            return (_bootSector.TotalSectors16 - _bootSector.FirstDataSector) / _bootSector.SectorsPerCluster;
        else
            return ((int)_bootSector.TotalSectors32 - _bootSector.FirstDataSector) / _bootSector.SectorsPerCluster;
    }

    private int CalculateFreeClusters()
    {
        var totalClusters = CalculateTotalClusters();
        var usedClusters = 0;
        
        for (int cluster = 2; cluster < totalClusters + 2; cluster++)
        {
            if (GetFatEntry(cluster) != 0)
                usedClusters++;
        }
        
        return totalClusters - usedClusters;
    }

    private void LoadFileSystem()
    {
        LoadBootSector();
        LoadFatTable();
        LoadRootDirectory();
    }

    private void LoadBootSector()
    {
        try
        {
            // Check if boot sector exists
            if (!_diskContainer.SectorExists(0, 0, 1))
            {
                // Create default boot sector for unformatted disk
                _bootSector = CreateDefaultBootSector();
                return;
            }
            
            var bootSectorData = _diskContainer.ReadSector(0, 0, 1);
            
            // Check if boot sector is valid (not all zeros or empty)
            if (bootSectorData.All(b => b == 0) || bootSectorData.Length < 512)
            {
                _bootSector = CreateDefaultBootSector();
                return;
            }
            
            _bootSector = Fat12BootSector.Parse(bootSectorData);
            
            // If parsing fails, use default
            if (!_bootSector.IsValid)
            {
                _bootSector = CreateDefaultBootSector();
            }
        }
        catch (Exception)
        {
            // If any error occurs, use default boot sector
            _bootSector = CreateDefaultBootSector();
        }
    }

    private Fat12BootSector CreateDefaultBootSector()
    {
        // DSK format uses different geometry than standard FAT12 floppy
        return new Fat12BootSector
        {
            BytesPerSector = 512,
            SectorsPerCluster = 1,
            ReservedSectors = 1,
            NumberOfFats = 2,
            RootEntries = 112, // Reduced for DSK format
            TotalSectors16 = (ushort)(_diskContainer.DiskType switch
            {
                DiskType.TwoD => 640,   // 40 cylinders × 1 head × 16 sectors
                DiskType.TwoDD => 1280, // 40 cylinders × 2 heads × 16 sectors
                DiskType.TwoHD => 2880, // 80 cylinders × 2 heads × 18 sectors
                _ => 1280
            }),
            SectorsPerFat = (ushort)(_diskContainer.DiskType switch
            {
                DiskType.TwoD => 2,
                DiskType.TwoDD => 3,
                DiskType.TwoHD => 9,
                _ => 3
            }),
            SectorsPerTrack = (ushort)(_diskContainer.DiskType switch
            {
                DiskType.TwoD => 16,  // DSK format uses 16 sectors per track
                DiskType.TwoDD => 16,
                DiskType.TwoHD => 18,
                _ => 16
            }),
            NumberOfHeads = (ushort)(_diskContainer.DiskType switch
            {
                DiskType.TwoD => 1,
                DiskType.TwoDD => 2,
                DiskType.TwoHD => 2,
                _ => 2
            }),
            VolumeLabel = "NO NAME    "
        };
    }

    private void LoadFatTable()
    {
        try
        {
            var fatSize = _bootSector.SectorsPerFat * _bootSector.BytesPerSector;
            _fatTable = new byte[fatSize];
            
            var fatStartSector = _bootSector.ReservedSectors;
            var sectorsRead = 0;
            
            for (int i = 0; i < _bootSector.SectorsPerFat; i++)
            {
                var cylinder = (fatStartSector + i) / (_bootSector.SectorsPerTrack * _bootSector.NumberOfHeads);
                var head = ((fatStartSector + i) % (_bootSector.SectorsPerTrack * _bootSector.NumberOfHeads)) / _bootSector.SectorsPerTrack;
                var sector = ((fatStartSector + i) % _bootSector.SectorsPerTrack) + 1;
                
                var sectorData = _diskContainer.ReadSector(cylinder, head, sector);
                Buffer.BlockCopy(sectorData, 0, _fatTable, sectorsRead * _bootSector.BytesPerSector, 
                                Math.Min(sectorData.Length, _bootSector.BytesPerSector));
                sectorsRead++;
            }
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"Failed to load FAT12 table: {ex.Message}", ex);
        }
    }

    private void LoadRootDirectory()
    {
        try
        {
            _rootDirectory.Clear();
            
            var rootDirStartSector = _bootSector.ReservedSectors + (_bootSector.NumberOfFats * _bootSector.SectorsPerFat);
            var rootDirSectors = (_bootSector.RootEntries * 32 + _bootSector.BytesPerSector - 1) / _bootSector.BytesPerSector;
            
            for (int i = 0; i < rootDirSectors; i++)
            {
                var cylinder = (rootDirStartSector + i) / (_bootSector.SectorsPerTrack * _bootSector.NumberOfHeads);
                var head = ((rootDirStartSector + i) % (_bootSector.SectorsPerTrack * _bootSector.NumberOfHeads)) / _bootSector.SectorsPerTrack;
                var sector = ((rootDirStartSector + i) % _bootSector.SectorsPerTrack) + 1;
                
                var sectorData = _diskContainer.ReadSector(cylinder, head, sector);
                
                // Parse directory entries (32 bytes each)
                for (int j = 0; j < sectorData.Length; j += 32)
                {
                    if (j + 32 > sectorData.Length) break;
                    
                    var entryData = new byte[32];
                    Buffer.BlockCopy(sectorData, j, entryData, 0, 32);
                    
                    var entry = Fat12DirectoryEntry.Parse(entryData);
                    if (entry.IsEndOfDirectory) break;
                    
                    _rootDirectory.Add(entry);
                }
            }
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"Failed to load FAT12 root directory: {ex.Message}", ex);
        }
    }

    private Fat12DirectoryEntry? FindFileEntry(string fileName)
    {
        var (name, ext) = ParseFileName(fileName);
        
        return _rootDirectory.FirstOrDefault(entry => 
            !entry.IsDeleted && 
            !entry.IsDirectory && 
            !entry.IsVolumeLabel &&
            entry.GetFileName().Equals(name, StringComparison.OrdinalIgnoreCase) &&
            entry.GetExtension().Equals(ext, StringComparison.OrdinalIgnoreCase));
    }

    private (string name, string ext) ParseFileName(string fileName)
    {
        var dotIndex = fileName.LastIndexOf('.');
        if (dotIndex == -1)
        {
            return (fileName.ToUpper(), "");
        }
        
        return (fileName.Substring(0, dotIndex).ToUpper(), fileName.Substring(dotIndex + 1).ToUpper());
    }

    private byte[] ReadFileData(Fat12DirectoryEntry entry, bool allowPartialRead)
    {
        var data = new List<byte>();
        int cluster = entry.FirstCluster;
        var remainingBytes = entry.FileSize;
        var chainLength = 0;

        while (cluster >= 2 && cluster <= 0xFEF && remainingBytes > 0)
        {
            chainLength++;
            if (chainLength > MaxClusterChainLength)
            {
                if (allowPartialRead)
                {
                    Console.WriteLine("Warning: Cluster chain too long, truncating file");
                    break;
                }
                throw new FileSystemException("Cluster chain too long - possible corruption");
            }

            try
            {
                var clusterData = ReadCluster(cluster);
                var bytesToTake = Math.Min(clusterData.Length, (int)remainingBytes);
                data.AddRange(clusterData.Take(bytesToTake));
                remainingBytes -= (uint)bytesToTake;
                
                cluster = GetNextCluster(cluster);
            }
            catch (Exception ex) when (allowPartialRead)
            {
                Console.WriteLine($"Warning: Error reading cluster {cluster}: {ex.Message}");
                break;
            }
        }

        return data.ToArray();
    }

    private byte[] ReadFileDataPartial(Fat12DirectoryEntry entry)
    {
        var data = new List<byte>();
        int cluster = entry.FirstCluster;
        var chainLength = 0;

        while (cluster >= 2 && cluster <= 0xFEF && chainLength < MaxClusterChainLength)
        {
            chainLength++;
            
            try
            {
                var clusterData = ReadCluster(cluster);
                data.AddRange(clusterData);
                cluster = GetNextCluster(cluster);
            }
            catch
            {
                // Skip corrupted clusters
                cluster = GetNextCluster(cluster);
            }
        }

        return data.ToArray();
    }

    private byte[] ReadCluster(int clusterNumber)
    {
        var firstDataSector = _bootSector.ReservedSectors + 
                             (_bootSector.NumberOfFats * _bootSector.SectorsPerFat) +
                             ((_bootSector.RootEntries * 32 + _bootSector.BytesPerSector - 1) / _bootSector.BytesPerSector);
        
        var clusterSector = firstDataSector + (clusterNumber - 2) * _bootSector.SectorsPerCluster;
        var clusterData = new List<byte>();
        
        for (int i = 0; i < _bootSector.SectorsPerCluster; i++)
        {
            var sectorNumber = clusterSector + i;
            var cylinder = sectorNumber / (_bootSector.SectorsPerTrack * _bootSector.NumberOfHeads);
            var head = (sectorNumber % (_bootSector.SectorsPerTrack * _bootSector.NumberOfHeads)) / _bootSector.SectorsPerTrack;
            var sector = (sectorNumber % _bootSector.SectorsPerTrack) + 1;
            
            var sectorData = _diskContainer.ReadSector(cylinder, head, sector);
            clusterData.AddRange(sectorData);
        }
        
        return clusterData.ToArray();
    }

    private int GetNextCluster(int currentCluster)
    {
        if (currentCluster < 2 || currentCluster >= 0xFF0)
            return 0xFFF; // End of chain
            
        // FAT12 stores 12-bit values, so we need to handle odd/even clusters differently
        var fatOffset = currentCluster + (currentCluster / 2);
        
        if (fatOffset + 1 >= _fatTable.Length)
            return 0xFFF;
            
        int nextCluster;
        if (currentCluster % 2 == 0)
        {
            // Even cluster: use lower 12 bits
            nextCluster = _fatTable[fatOffset] | ((_fatTable[fatOffset + 1] & 0x0F) << 8);
        }
        else
        {
            // Odd cluster: use upper 12 bits
            nextCluster = (_fatTable[fatOffset] >> 4) | (_fatTable[fatOffset + 1] << 4);
        }
        
        return nextCluster & 0xFFF;
    }

    private static HuBasicFileMode GetFileMode(Fat12DirectoryEntry entry)
    {
        // FAT12 doesn't have the same mode system as Hu-BASIC
        // We'll map based on file extension and attributes
        var ext = entry.GetExtension().ToLower();
        
        if (ext == "exe" || ext == "com" || ext == "bin")
        {
            return HuBasicFileMode.Binary;
        }
        else if (ext == "bas")
        {
            return HuBasicFileMode.Basic;
        }
        else if (ext == "txt" || ext == "asc")
        {
            return HuBasicFileMode.Ascii;
        }
        
        // Default to binary for unknown file types
        return HuBasicFileMode.Binary;
    }

    private string GetDosFileName(Fat12DirectoryEntry entry)
    {
        if (string.IsNullOrEmpty(entry.Extension))
            return entry.Name;
        return $"{entry.Name}.{entry.Extension}";
    }

    private DateTime ConvertDosDateTime(ushort date, ushort time)
    {
        if (date == 0) return DateTime.MinValue;
        
        try
        {
            var year = 1980 + ((date >> 9) & 0x7F);
            var month = (date >> 5) & 0x0F;
            var day = date & 0x1F;
            
            var hour = (time >> 11) & 0x1F;
            var minute = (time >> 5) & 0x3F;
            var second = (time & 0x1F) * 2;
            
            return new DateTime(year, month, day, hour, minute, second);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private int GetFatEntry(int cluster)
    {
        if (cluster < 2 || _fatTable.Length == 0) return 0;
        
        // FAT12では1.5バイトエントリを使用
        var offset = cluster + (cluster / 2);
        if (offset + 1 >= _fatTable.Length) return 0;
        
        int value;
        if (cluster % 2 == 0)
        {
            // 偶数クラスタ: 下位12ビット
            value = _fatTable[offset] | ((_fatTable[offset + 1] & 0x0F) << 8);
        }
        else
        {
            // 奇数クラスタ: 上位12ビット
            value = (_fatTable[offset] >> 4) | (_fatTable[offset + 1] << 4);
        }
        
        return value & 0xFFF;
    }
}
