using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

public class Fat12FileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private Fat12BootSector _bootSector = new();
    private byte[] _fatTable = Array.Empty<byte>();
    private readonly List<Fat12DirectoryEntry> _rootDirectory = new();
    
    // FAT12 constants
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB safety limit
    private const int MaxClusterChainLength = 4000; // Prevent infinite loops
    
    public Fat12FileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer ?? throw new ArgumentNullException(nameof(diskContainer));
        LoadFileSystem();
    }

    public void Format()
    {
        throw new NotImplementedException("FAT12 filesystem formatting not yet implemented");
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
            
            files.Add(new FileEntry
            {
                FileName = fileName,
                Extension = extension,
                Size = entry.FileSize,
                LoadAddress = 0,
                ExecAddress = 0,
                Mode = GetFileMode(entry),
                ModifiedDate = entry.GetModifiedDate()
            });
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
        throw new NotImplementedException("FAT12 file writing not yet implemented");
    }

    public void DeleteFile(string fileName)
    {
        throw new NotImplementedException("FAT12 file deletion not yet implemented");
    }

    public BootSectorInfo ReadBootSector()
    {
        return new BootSectorInfo
        {
            Label = _bootSector.VolumeLabel.Trim(),
            LoadAddress = 0,
            ExecAddress = 0,
            Size = _bootSector.BytesPerSector,
            ModifiedDate = DateTime.MinValue
        };
    }

    public void WriteBootSector(string label, byte[] bootCode)
    {
        throw new NotImplementedException("FAT12 boot sector writing not yet implemented");
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
            var bootSectorData = _diskContainer.ReadSector(0, 0, 1);
            _bootSector = Fat12BootSector.Parse(bootSectorData);
            
            // Validate FAT12 signature
            if (!_bootSector.IsValidFat12())
            {
                throw new FileSystemException("Invalid FAT12 boot sector");
            }
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"Failed to load FAT12 boot sector: {ex.Message}", ex);
        }
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
        var cluster = entry.FirstCluster;
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
                var bytesToTake = Math.Min(clusterData.Length, remainingBytes);
                data.AddRange(clusterData.Take(bytesToTake));
                remainingBytes -= bytesToTake;
                
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
        var cluster = entry.FirstCluster;
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

    private static byte GetFileMode(Fat12DirectoryEntry entry)
    {
        byte mode = 0;
        
        // FAT12 doesn't have the same mode system as Hu-BASIC
        // We'll map based on file extension and attributes
        var ext = entry.GetExtension().ToLower();
        
        if (ext == "exe" || ext == "com" || ext == "bin")
        {
            mode |= 0x01; // Binary
        }
        else if (ext == "bas")
        {
            mode |= 0x02; // BASIC
        }
        else if (ext == "txt" || ext == "asc")
        {
            mode |= 0x04; // ASCII
        }
        
        if ((entry.Attributes & 0x01) != 0) // Read-only
        {
            mode |= 0x40;
        }
        
        if ((entry.Attributes & 0x02) != 0) // Hidden
        {
            mode |= 0x10;
        }
        
        return mode;
    }
}

// FAT12 Boot Sector structure
public class Fat12BootSector
{
    public byte[] JumpInstruction { get; set; } = new byte[3];
    public string OemName { get; set; } = "";
    public ushort BytesPerSector { get; set; }
    public byte SectorsPerCluster { get; set; }
    public ushort ReservedSectors { get; set; }
    public byte NumberOfFats { get; set; }
    public ushort RootEntries { get; set; }
    public ushort TotalSectors16 { get; set; }
    public byte MediaType { get; set; }
    public ushort SectorsPerFat { get; set; }
    public ushort SectorsPerTrack { get; set; }
    public ushort NumberOfHeads { get; set; }
    public uint HiddenSectors { get; set; }
    public uint TotalSectors32 { get; set; }
    public byte DriveNumber { get; set; }
    public byte Reserved1 { get; set; }
    public byte BootSignature { get; set; }
    public uint VolumeId { get; set; }
    public string VolumeLabel { get; set; } = "";
    public string FileSystemType { get; set; } = "";

    public static Fat12BootSector Parse(byte[] data)
    {
        if (data.Length < 512)
            throw new ArgumentException("Boot sector data too short");

        var bootSector = new Fat12BootSector();
        
        Buffer.BlockCopy(data, 0, bootSector.JumpInstruction, 0, 3);
        bootSector.OemName = Encoding.ASCII.GetString(data, 3, 8).Trim('\0');
        bootSector.BytesPerSector = BitConverter.ToUInt16(data, 11);
        bootSector.SectorsPerCluster = data[13];
        bootSector.ReservedSectors = BitConverter.ToUInt16(data, 14);
        bootSector.NumberOfFats = data[16];
        bootSector.RootEntries = BitConverter.ToUInt16(data, 17);
        bootSector.TotalSectors16 = BitConverter.ToUInt16(data, 19);
        bootSector.MediaType = data[21];
        bootSector.SectorsPerFat = BitConverter.ToUInt16(data, 22);
        bootSector.SectorsPerTrack = BitConverter.ToUInt16(data, 24);
        bootSector.NumberOfHeads = BitConverter.ToUInt16(data, 26);
        bootSector.HiddenSectors = BitConverter.ToUInt32(data, 28);
        bootSector.TotalSectors32 = BitConverter.ToUInt32(data, 32);
        bootSector.DriveNumber = data[36];
        bootSector.Reserved1 = data[37];
        bootSector.BootSignature = data[38];
        bootSector.VolumeId = BitConverter.ToUInt32(data, 39);
        bootSector.VolumeLabel = Encoding.ASCII.GetString(data, 43, 11).Trim();
        bootSector.FileSystemType = Encoding.ASCII.GetString(data, 54, 8).Trim();

        return bootSector;
    }

    public bool IsValidFat12()
    {
        return BytesPerSector > 0 && 
               SectorsPerCluster > 0 && 
               NumberOfFats > 0 && 
               RootEntries > 0 &&
               (FileSystemType.Contains("FAT12") || FileSystemType.Contains("FAT"));
    }
}

// FAT12 Directory Entry structure
public class Fat12DirectoryEntry
{
    public byte[] FileName { get; set; } = new byte[8];
    public byte[] Extension { get; set; } = new byte[3];
    public byte Attributes { get; set; }
    public byte Reserved { get; set; }
    public byte CreationTimeTenths { get; set; }
    public ushort CreationTime { get; set; }
    public ushort CreationDate { get; set; }
    public ushort LastAccessDate { get; set; }
    public ushort FirstClusterHigh { get; set; }
    public ushort WriteTime { get; set; }
    public ushort WriteDate { get; set; }
    public ushort FirstCluster { get; set; }
    public uint FileSize { get; set; }

    public bool IsDeleted => FileName[0] == 0xE5;
    public bool IsEndOfDirectory => FileName[0] == 0x00;
    public bool IsDirectory => (Attributes & 0x10) != 0;
    public bool IsVolumeLabel => (Attributes & 0x08) != 0;

    public static Fat12DirectoryEntry Parse(byte[] data)
    {
        if (data.Length < 32)
            throw new ArgumentException("Directory entry data too short");

        var entry = new Fat12DirectoryEntry();
        
        Buffer.BlockCopy(data, 0, entry.FileName, 0, 8);
        Buffer.BlockCopy(data, 8, entry.Extension, 0, 3);
        entry.Attributes = data[11];
        entry.Reserved = data[12];
        entry.CreationTimeTenths = data[13];
        entry.CreationTime = BitConverter.ToUInt16(data, 14);
        entry.CreationDate = BitConverter.ToUInt16(data, 16);
        entry.LastAccessDate = BitConverter.ToUInt16(data, 18);
        entry.FirstClusterHigh = BitConverter.ToUInt16(data, 20);
        entry.WriteTime = BitConverter.ToUInt16(data, 22);
        entry.WriteDate = BitConverter.ToUInt16(data, 24);
        entry.FirstCluster = BitConverter.ToUInt16(data, 26);
        entry.FileSize = BitConverter.ToUInt32(data, 28);

        return entry;
    }

    public string GetFileName()
    {
        return Encoding.ASCII.GetString(FileName).Trim();
    }

    public string GetExtension()
    {
        return Encoding.ASCII.GetString(Extension).Trim();
    }

    public DateTime GetModifiedDate()
    {
        if (WriteDate == 0) return DateTime.MinValue;
        
        try
        {
            var year = 1980 + ((WriteDate >> 9) & 0x7F);
            var month = (WriteDate >> 5) & 0x0F;
            var day = WriteDate & 0x1F;
            
            var hour = (WriteTime >> 11) & 0x1F;
            var minute = (WriteTime >> 5) & 0x3F;
            var second = (WriteTime & 0x1F) * 2;
            
            return new DateTime(year, month, day, hour, minute, second);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}