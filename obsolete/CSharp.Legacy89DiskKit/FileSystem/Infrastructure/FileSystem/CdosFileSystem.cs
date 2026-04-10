using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

/// <summary>
/// Implements CDOS (Club DOS) file system operations
/// </summary>
public class CdosFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly CdosConfiguration _config;
    private readonly ICharacterEncoder? _characterEncoder;
    private readonly List<CdosFileEntry> _directoryCache;
    private bool _directoryCacheValid;

    /// <summary>
    /// Initializes a new instance of the CdosFileSystem class
    /// </summary>
    public CdosFileSystem(IDiskContainer diskContainer, ICharacterEncoder? characterEncoder = null)
    {
        _diskContainer = diskContainer ?? throw new ArgumentNullException(nameof(diskContainer));
        _config = CdosConfiguration.GetConfiguration(diskContainer.DiskType);
        _characterEncoder = characterEncoder;
        _directoryCache = new List<CdosFileEntry>();
        _directoryCacheValid = false;
    }

    /// <inheritdoc/>
    public IDiskContainer DiskContainer => _diskContainer;

    /// <inheritdoc/>
    public bool IsFormatted => CheckIfFormatted();

    private bool CheckIfFormatted()
    {
        try
        {
            var entries = GetCdosDirectoryEntries();
            // Check if we can read directory entries (formatted = directory area is readable)
            // An empty but formatted disk should return an empty list, not throw an exception
            return entries != null;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<FileEntry> GetFiles()
    {
        var cdosEntries = GetCdosDirectoryEntries();

        foreach (var entry in cdosEntries.Where(e => e.IsValid))
        {
            // CDOS doesn't have file modes like HuBasic
            var mode = HuBasicFileMode.Binary;
            
            // Set attributes based on CDOS attributes
            var attributes = new HuBasicFileAttributes(
                IsDirectory: false,
                IsReadOnly: entry.Attributes.HasFlag(CdosFileAttributes.ReadOnly),
                IsVerify: false,
                IsHidden: entry.Attributes.HasFlag(CdosFileAttributes.Hidden),
                IsBinary: true,
                IsBasic: entry.Extension.Trim().Equals("BAS", StringComparison.OrdinalIgnoreCase),
                IsAscii: entry.Extension.Trim().Equals("TXT", StringComparison.OrdinalIgnoreCase)
            );

            yield return new FileEntry(
                FileName: entry.FileName.TrimEnd(),
                Extension: entry.Extension.TrimEnd(),
                Mode: mode,
                Attributes: attributes,
                Size: (int)entry.FileSize,
                LoadAddress: entry.LoadAddress,
                ExecuteAddress: entry.ExecutionAddress,
                ModifiedDate: DateTime.MinValue, // CDOS doesn't store timestamps
                IsProtected: entry.Attributes.HasFlag(CdosFileAttributes.ReadOnly)
            );
        }
    }

    /// <inheritdoc/>
    public bool FileExists(string fileName)
    {
        var (name, extension) = CdosFileNameValidator.NormalizeFileName(fileName);
        var cdosEntries = GetCdosDirectoryEntries();
        
        return cdosEntries.Any(e => 
            e.IsValid && 
            e.FileName.TrimEnd() == name && 
            e.Extension.TrimEnd() == extension);
    }

    /// <inheritdoc/>
    public long GetFileSize(string fileName)
    {
        var (name, extension) = CdosFileNameValidator.NormalizeFileName(fileName);
        var cdosEntries = GetCdosDirectoryEntries();
        
        var fileEntry = cdosEntries.FirstOrDefault(e => 
            e.IsValid && 
            e.FileName.TrimEnd() == name && 
            e.Extension.TrimEnd() == extension);

        if (fileEntry == null)
            throw new Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException($"File not found: {fileName}");

        return fileEntry.FileSize;
    }

    /// <inheritdoc/>
    public void ExportFile(string fileName, string destinationPath)
    {
        var (name, extension) = CdosFileNameValidator.NormalizeFileName(fileName);
        var cdosEntries = GetCdosDirectoryEntries();
        
        var fileEntry = cdosEntries.FirstOrDefault(e => 
            e.IsValid && 
            e.FileName.TrimEnd() == name && 
            e.Extension.TrimEnd() == extension);

        if (fileEntry == null)
            throw new Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException($"File not found: {fileName}");

        using var output = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
        
        // Calculate file position and read data
        var data = ReadFileData(fileEntry);
        output.Write(data, 0, data.Length);
    }

    /// <inheritdoc/>
    public void ImportFile(string sourcePath, string destinationFileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Disk container is read-only");

        // Normalize and validate filename
        if (!CdosFileNameValidator.IsValidFileName(destinationFileName))
            throw new ArgumentException($"Invalid CDOS filename: {destinationFileName}");

        var (name, extension) = CdosFileNameValidator.FormatForDirectory(destinationFileName);
        
        // Check if file already exists
        if (FileExists(destinationFileName))
            throw new InvalidOperationException($"File already exists: {destinationFileName}");

        var fileInfo = new FileInfo(sourcePath);
        if (!fileInfo.Exists)
            throw new System.IO.FileNotFoundException($"Source file not found: {sourcePath}");

        if (fileInfo.Length > uint.MaxValue)
            throw new InvalidOperationException($"File too large for CDOS: {fileInfo.Length} bytes");

        // Find free directory entry
        var freeEntry = FindFreeDirectoryEntry();
        if (freeEntry == null)
            throw new InvalidOperationException("Directory is full");

        // Find contiguous free space for the file
        var fileSize = (uint)fileInfo.Length;
        var (startTrack, startSector) = FindContiguousFreeSpace(fileSize);
        if (startTrack == -1)
            throw new InvalidOperationException("Not enough contiguous free space on disk");

        // Read file data
        var fileData = File.ReadAllBytes(sourcePath);

        // Write file data to disk
        WriteFileData((byte)startTrack, (byte)startSector, fileData);

        // Create directory entry
        var newEntry = new CdosFileEntry
        {
            FileName = name,
            Extension = extension,
            Attributes = CdosFileAttributes.None,
            FileSize = fileSize,
            StartTrack = (byte)startTrack,
            StartSector = (byte)startSector,
            LoadAddress = 0,
            ExecutionAddress = 0,
            IsDeleted = false,
            IsEmpty = false
        };

        // Write directory entry
        WriteDirectoryEntry(freeEntry.Value, newEntry);
        InvalidateDirectoryCache();
    }

    /// <inheritdoc/>
    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Disk container is read-only");

        var (name, extension) = CdosFileNameValidator.NormalizeFileName(fileName);
        var cdosEntries = GetCdosDirectoryEntries();
        
        var entryIndex = -1;
        for (int i = 0; i < cdosEntries.Count; i++)
        {
            var entry = cdosEntries[i];
            if (entry.IsValid && 
                entry.FileName.TrimEnd() == name && 
                entry.Extension.TrimEnd() == extension)
            {
                entryIndex = i;
                break;
            }
        }

        if (entryIndex == -1)
            throw new Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException($"File not found: {fileName}");

        // Mark file as deleted by setting first byte of filename to 0xE5
        var deletedEntry = cdosEntries[entryIndex] with { IsDeleted = true };
        WriteDirectoryEntry(entryIndex, deletedEntry);
        InvalidateDirectoryCache();
    }

    /// <inheritdoc/>
    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Disk container is read-only");

        // Clear directory area
        var emptyEntry = new byte[CdosFileEntry.EntrySize];
        Array.Fill(emptyEntry, (byte)0);

        var entriesPerSector = _config.SectorSize / CdosFileEntry.EntrySize;
        var totalSectors = (_config.MaxDirectoryEntries * CdosFileEntry.EntrySize) / _config.SectorSize;
        
        var track = _config.DirectoryStartTrack;
        var head = 0;
        var sector = _config.DirectoryStartSector;

        for (int i = 0; i < totalSectors; i++)
        {
            var sectorData = new byte[_config.SectorSize];
            _diskContainer.WriteSector(track, head, sector, sectorData);
            
            sector++;
            if (sector > _config.SectorsPerTrack)
            {
                sector = 1;
                head++;
                if (head >= _config.HeadCount)
                {
                    head = 0;
                    track++;
                }
            }
        }

        // Write boot sector if needed
        WriteBootSector();
        InvalidateDirectoryCache();
    }

    /// <inheritdoc/>
    public string GetVolumeLabel() => "CDOS DISK";

    /// <inheritdoc/>
    public void SetVolumeLabel(string label)
    {
        // CDOS doesn't support volume labels
    }

    /// <inheritdoc/>
    public void CreateDirectory(string directoryName)
    {
        throw new NotSupportedException("CDOS does not support directories");
    }

    /// <inheritdoc/>
    public void DeleteDirectory(string directoryName)
    {
        throw new NotSupportedException("CDOS does not support directories");
    }

    /// <inheritdoc/>
    public IEnumerable<FileEntry> GetFiles(string pattern)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            CdosFileNameValidator.WildcardToRegex(pattern),
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return GetFiles().Where(f => 
        {
            var fullName = string.IsNullOrEmpty(f.Extension) ? f.FileName : $"{f.FileName}.{f.Extension}";
            return regex.IsMatch(fullName);
        });
    }

    /// <inheritdoc/>
    public FileEntry? GetFile(string fileName)
    {
        var (name, extension) = CdosFileNameValidator.NormalizeFileName(fileName);
        return GetFiles().FirstOrDefault(f => 
            f.FileName.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            f.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public byte[] ReadFile(string fileName)
    {
        return ReadFile(fileName, false);
    }

    /// <inheritdoc/>
    public byte[] ReadFile(string fileName, bool allowPartialRead)
    {
        var (name, extension) = CdosFileNameValidator.NormalizeFileName(fileName);
        var cdosEntries = GetCdosDirectoryEntries();
        
        var fileEntry = cdosEntries.FirstOrDefault(e => 
            e.IsValid && 
            e.FileName.TrimEnd() == name && 
            e.Extension.TrimEnd() == extension);

        if (fileEntry == null)
            throw new Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException($"File not found: {fileName}");

        return ReadFileData(fileEntry);
    }

    /// <inheritdoc/>
    public void WriteFile(string fileName, byte[] data, bool isText = false, ushort loadAddress = 0, ushort execAddress = 0)
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempPath, data);
            ImportFile(tempPath, fileName);
            
            // Update load/exec addresses if specified
            if (loadAddress != 0 || execAddress != 0)
            {
                UpdateFileAddresses(fileName, loadAddress, execAddress);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <inheritdoc/>
    public BootSector GetBootSector()
    {
        // CDOS boot sector info
        return new BootSector(
            IsBootable: true,
            Label: "CDOS DISK",
            Extension: "",
            Size: 256,
            LoadAddress: 0xC000,
            ExecuteAddress: 0xC000,
            ModifiedDate: DateTime.MinValue,
            StartSector: 1
        );
    }

    /// <inheritdoc/>
    public void WriteBootSector(BootSector bootSector)
    {
        // CDOS doesn't support custom boot sector modification
        throw new NotSupportedException("CDOS does not support custom boot sector modification");
    }

    /// <inheritdoc/>
    public HuBasicFileSystemInfo GetFileSystemInfo()
    {
        var totalSectors = CalculateTotalSectors();
        var usedSectors = CalculateUsedSectors();
        var freeSectors = totalSectors - usedSectors;
        var sectorSize = _config.SectorSize;

        return new HuBasicFileSystemInfo(
            TotalClusters: totalSectors,
            FreeClusters: freeSectors,
            ClusterSize: sectorSize,
            SectorSize: sectorSize
        );
    }

    #region Private Methods

    private List<CdosFileEntry> GetCdosDirectoryEntries()
    {
        if (_directoryCacheValid)
            return _directoryCache;

        _directoryCache.Clear();
        
        var entriesPerSector = _config.SectorSize / CdosFileEntry.EntrySize;
        var totalSectors = (_config.MaxDirectoryEntries * CdosFileEntry.EntrySize) / _config.SectorSize;
        
        var track = _config.DirectoryStartTrack;
        var head = 0;
        var sector = _config.DirectoryStartSector;

        for (int i = 0; i < totalSectors; i++)
        {
            var sectorData = _diskContainer.ReadSector(track, head, sector);
            
            for (int j = 0; j < entriesPerSector && _directoryCache.Count < _config.MaxDirectoryEntries; j++)
            {
                var offset = j * CdosFileEntry.EntrySize;
                var entry = ParseDirectoryEntry(sectorData, offset);
                _directoryCache.Add(entry);
            }

            sector++;
            if (sector > _config.SectorsPerTrack)
            {
                sector = 1;
                head++;
                if (head >= _config.HeadCount)
                {
                    head = 0;
                    track++;
                }
            }
        }

        _directoryCacheValid = true;
        return _directoryCache;
    }

    private CdosFileEntry ParseDirectoryEntry(byte[] data, int offset)
    {
        // Check if entry is deleted or empty
        var firstByte = data[offset];
        var isDeleted = firstByte == CdosFileEntry.DeletedMarker;
        var isEmpty = firstByte == CdosFileEntry.EmptyMarker;

        // Parse filename and extension
        var fileName = Encoding.ASCII.GetString(data, offset, 8);
        var extension = Encoding.ASCII.GetString(data, offset + 8, 3);

        // Parse attributes
        var attributes = (CdosFileAttributes)data[offset + 0x0B];

        // Parse file size (32-bit little endian at offset 0x0D)
        var fileSize = BitConverter.ToUInt32(data, offset + 0x0D);

        // Parse start location
        var startTrack = data[offset + 0x11];
        var startSector = data[offset + 0x12];

        // Parse load and execution addresses
        var loadAddress = BitConverter.ToUInt16(data, offset + 0x13);
        var executionAddress = BitConverter.ToUInt16(data, offset + 0x15);

        return new CdosFileEntry
        {
            FileName = fileName,
            Extension = extension,
            Attributes = attributes,
            FileSize = fileSize,
            StartTrack = startTrack,
            StartSector = startSector,
            LoadAddress = loadAddress,
            ExecutionAddress = executionAddress,
            IsDeleted = isDeleted,
            IsEmpty = isEmpty
        };
    }

    private void WriteDirectoryEntry(int index, CdosFileEntry entry)
    {
        var entriesPerSector = _config.SectorSize / CdosFileEntry.EntrySize;
        var sectorIndex = index / entriesPerSector;
        var entryOffset = (index % entriesPerSector) * CdosFileEntry.EntrySize;
        
        var track = _config.DirectoryStartTrack;
        var head = 0;
        var sector = _config.DirectoryStartSector + sectorIndex;
        
        while (sector > _config.SectorsPerTrack)
        {
            sector -= _config.SectorsPerTrack;
            head++;
            if (head >= _config.HeadCount)
            {
                head = 0;
                track++;
            }
        }

        // Read current sector
        var sectorData = _diskContainer.ReadSector(track, head, sector);
        
        // Update entry
        if (entry.IsDeleted)
        {
            sectorData[entryOffset] = CdosFileEntry.DeletedMarker;
        }
        else if (entry.IsEmpty)
        {
            sectorData[entryOffset] = CdosFileEntry.EmptyMarker;
        }
        else
        {
            var nameBytes = Encoding.ASCII.GetBytes(entry.FileName.PadRight(8));
            Array.Copy(nameBytes, 0, sectorData, entryOffset, 8);
        }
        
        var extBytes = Encoding.ASCII.GetBytes(entry.Extension.PadRight(3));
        Array.Copy(extBytes, 0, sectorData, entryOffset + 8, 3);
        
        sectorData[entryOffset + 0x0B] = (byte)entry.Attributes;
        sectorData[entryOffset + 0x0C] = 0; // Reserved
        
        // Write file size (32-bit little endian)
        var sizeBytes = BitConverter.GetBytes(entry.FileSize);
        Array.Copy(sizeBytes, 0, sectorData, entryOffset + 0x0D, 4);
        
        sectorData[entryOffset + 0x11] = entry.StartTrack;
        sectorData[entryOffset + 0x12] = entry.StartSector;
        
        // Write load and execution addresses
        var loadBytes = BitConverter.GetBytes(entry.LoadAddress);
        var execBytes = BitConverter.GetBytes(entry.ExecutionAddress);
        Array.Copy(loadBytes, 0, sectorData, entryOffset + 0x13, 2);
        Array.Copy(execBytes, 0, sectorData, entryOffset + 0x15, 2);
        
        // Clear remaining bytes
        for (int i = 0x17; i < CdosFileEntry.EntrySize; i++)
        {
            sectorData[entryOffset + i] = 0;
        }
        
        // Write back sector
        _diskContainer.WriteSector(track, head, sector, sectorData);
    }

    private byte[] ReadFileData(CdosFileEntry entry)
    {
        var result = new byte[entry.FileSize];
        var bytesRead = 0;
        
        var track = entry.StartTrack;
        var head = 0;
        var sector = entry.StartSector;

        while (bytesRead < entry.FileSize)
        {
            // Handle mixed sector sizes for 2HD
            var sectorSize = GetSectorSize(track);
            var sectorData = _diskContainer.ReadSector(track, head, sector);
            
            var bytesToCopy = Math.Min(sectorSize, (int)(entry.FileSize - bytesRead));
            Array.Copy(sectorData, 0, result, bytesRead, bytesToCopy);
            bytesRead += bytesToCopy;

            // Move to next sector
            sector++;
            var sectorsPerTrack = GetSectorsPerTrack(track);
            if (sector > sectorsPerTrack)
            {
                sector = 1;
                head++;
                if (head >= _config.HeadCount)
                {
                    head = 0;
                    track++;
                }
            }
        }

        return result;
    }

    private void WriteFileData(byte startTrack, byte startSector, byte[] data)
    {
        var bytesWritten = 0;
        var track = startTrack;
        var head = 0;
        var sector = startSector;

        while (bytesWritten < data.Length)
        {
            var sectorSize = GetSectorSize(track);
            var sectorData = new byte[sectorSize];
            
            var bytesToWrite = Math.Min(sectorSize, data.Length - bytesWritten);
            Array.Copy(data, bytesWritten, sectorData, 0, bytesToWrite);
            
            _diskContainer.WriteSector(track, head, sector, sectorData);
            bytesWritten += bytesToWrite;

            // Move to next sector
            sector++;
            var sectorsPerTrack = GetSectorsPerTrack(track);
            if (sector > sectorsPerTrack)
            {
                sector = 1;
                head++;
                if (head >= _config.HeadCount)
                {
                    head = 0;
                    track++;
                }
            }
        }
    }

    private int GetSectorSize(int track)
    {
        if (_config.IsMixedSectorSize && track == 0)
            return _config.Track0SectorSize;
        return _config.SectorSize;
    }

    private int GetSectorsPerTrack(int track)
    {
        if (_config.IsMixedSectorSize && track == 0)
            return _config.Track0SectorsPerTrack;
        return _config.SectorsPerTrack;
    }

    private int? FindFreeDirectoryEntry()
    {
        var entries = GetCdosDirectoryEntries();
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].IsEmpty || entries[i].IsDeleted)
                return i;
        }
        return null;
    }

    private (int track, int sector) FindContiguousFreeSpace(uint requiredBytes)
    {
        var usedSectors = GetUsedSectors();
        var requiredSectors = (int)((requiredBytes + _config.SectorSize - 1) / _config.SectorSize);
        
        // Start searching after directory area
        var track = _config.DirectoryStartTrack + 1;
        var head = 0;
        var sector = 1;
        var consecutiveFree = 0;
        var startTrack = -1;
        var startSector = -1;

        while (track < _config.TrackCount)
        {
            var sectorKey = GetSectorKey(track, head, sector);
            
            if (!usedSectors.Contains(sectorKey))
            {
                if (consecutiveFree == 0)
                {
                    startTrack = track;
                    startSector = sector;
                }
                consecutiveFree++;
                
                if (consecutiveFree >= requiredSectors)
                {
                    return (startTrack, startSector);
                }
            }
            else
            {
                consecutiveFree = 0;
                startTrack = -1;
                startSector = -1;
            }

            // Move to next sector
            sector++;
            var sectorsPerTrack = GetSectorsPerTrack(track);
            if (sector > sectorsPerTrack)
            {
                sector = 1;
                head++;
                if (head >= _config.HeadCount)
                {
                    head = 0;
                    track++;
                }
            }
        }

        return (-1, -1);
    }

    private HashSet<string> GetUsedSectors()
    {
        var usedSectors = new HashSet<string>();
        var entries = GetCdosDirectoryEntries();

        // Mark directory sectors as used
        var dirSectors = (_config.MaxDirectoryEntries * CdosFileEntry.EntrySize) / _config.SectorSize;
        var track = _config.DirectoryStartTrack;
        var head = 0;
        var sector = _config.DirectoryStartSector;

        for (int i = 0; i < dirSectors; i++)
        {
            usedSectors.Add(GetSectorKey(track, head, sector));
            sector++;
            if (sector > _config.SectorsPerTrack)
            {
                sector = 1;
                head++;
                if (head >= _config.HeadCount)
                {
                    head = 0;
                    track++;
                }
            }
        }

        // Mark file sectors as used
        foreach (var entry in entries.Where(e => e.IsValid))
        {
            track = entry.StartTrack;
            head = 0;
            sector = entry.StartSector;
            var bytesProcessed = 0;

            while (bytesProcessed < entry.FileSize)
            {
                usedSectors.Add(GetSectorKey(track, head, sector));
                
                var sectorSize = GetSectorSize(track);
                bytesProcessed += sectorSize;

                sector++;
                var sectorsPerTrack = GetSectorsPerTrack(track);
                if (sector > sectorsPerTrack)
                {
                    sector = 1;
                    head++;
                    if (head >= _config.HeadCount)
                    {
                        head = 0;
                        track++;
                    }
                }
            }
        }

        // Mark boot sector as used
        usedSectors.Add(GetSectorKey(0, 0, 1));

        return usedSectors;
    }

    private string GetSectorKey(int track, int head, int sector)
    {
        return $"{track:D3}-{head:D1}-{sector:D3}";
    }

    private int CalculateTotalSectors()
    {
        var total = 0;
        for (int track = 0; track < _config.TrackCount; track++)
        {
            var sectorsPerTrack = GetSectorsPerTrack(track);
            total += sectorsPerTrack * _config.HeadCount;
        }
        return total;
    }

    private int CalculateUsedSectors()
    {
        return GetUsedSectors().Count;
    }

    private void UpdateFileAddresses(string fileName, ushort loadAddress, ushort execAddress)
    {
        var (name, extension) = CdosFileNameValidator.NormalizeFileName(fileName);
        var entries = GetCdosDirectoryEntries();
        
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.IsValid && 
                entry.FileName.TrimEnd() == name && 
                entry.Extension.TrimEnd() == extension)
            {
                var updatedEntry = entry with
                {
                    LoadAddress = loadAddress,
                    ExecutionAddress = execAddress
                };
                WriteDirectoryEntry(i, updatedEntry);
                InvalidateDirectoryCache();
                break;
            }
        }
    }

    private void WriteBootSector()
    {
        // Create a simple boot sector
        var bootSector = new byte[256];
        
        // Simple boot code that displays a message
        var bootCode = new byte[]
        {
            0x3E, 0x0D,       // LD A, 0x0D (CR)
            0xCD, 0x00, 0x00, // CALL 0x0000 (BIOS call - implementation specific)
            0x3E, 0x0A,       // LD A, 0x0A (LF)
            0xCD, 0x00, 0x00, // CALL 0x0000
            0x21, 0x20, 0xC0, // LD HL, 0xC020 (message location)
            0x7E,             // LD A, (HL)
            0xB7,             // OR A
            0xC8,             // RET Z
            0xCD, 0x00, 0x00, // CALL 0x0000
            0x23,             // INC HL
            0x18, 0xF8,       // JR -8
        };
        
        Array.Copy(bootCode, bootSector, bootCode.Length);
        
        // Add boot message
        var message = Encoding.ASCII.GetBytes("CDOS SYSTEM\0");
        Array.Copy(message, 0, bootSector, 0x20, message.Length);
        
        _diskContainer.WriteSector(0, 0, 1, bootSector);
    }

    private void InvalidateDirectoryCache()
    {
        _directoryCacheValid = false;
    }

    #endregion
}