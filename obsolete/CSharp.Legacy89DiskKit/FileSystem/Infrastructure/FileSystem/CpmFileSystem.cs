using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using Legacy89DiskKit.FileSystem.Infrastructure.Utility;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

/// <summary>
/// Implements CP/M file system operations
/// </summary>
public class CpmFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly CpmConfiguration _config;
    private readonly ICharacterEncoder? _characterEncoder;
    private readonly List<CpmFileEntry> _directoryCache;
    private bool _directoryCacheValid;

    /// <summary>
    /// Initializes a new instance of the CpmFileSystem class
    /// </summary>
    public CpmFileSystem(IDiskContainer diskContainer, ICharacterEncoder? characterEncoder = null)
    {
        _diskContainer = diskContainer ?? throw new ArgumentNullException(nameof(diskContainer));
        _config = CpmConfiguration.GetConfiguration(diskContainer.DiskType);
        _characterEncoder = characterEncoder;
        _directoryCache = new List<CpmFileEntry>();
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
            var entries = GetCpmDirectoryEntries();
            return entries.Any(e => !e.IsEmpty);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<FileEntry> GetFiles()
    {
        var cpmEntries = GetCpmDirectoryEntries();
        
        // Group entries by filename to handle multi-extent files
        var fileGroups = cpmEntries
            .Where(e => e.IsValid && !string.IsNullOrWhiteSpace(e.FileName.Trim()))
            .GroupBy(e => new { e.UserNumber, e.FileName, e.Extension });

        foreach (var group in fileGroups)
        {
            var entries = group.OrderBy(e => e.ExtentNumber).ToList();
            var firstEntry = entries.First();
            
            // Calculate total file size across all extents
            long totalSize = 0;
            foreach (var entry in entries)
            {
                // Each extent can hold up to 16K (128 records * 128 bytes)
                if (entry == entries.Last())
                {
                    // Last extent: use actual record count
                    totalSize += entry.RecordCount * 128L;
                }
                else
                {
                    // Full extent
                    totalSize += 16384;
                }
            }

            // CP/M doesn't have file modes like HuBasic, so use Binary as default
            var mode = HuBasicFileMode.Binary;
            var attributes = new HuBasicFileAttributes(
                IsDirectory: false,
                IsReadOnly: firstEntry.IsReadOnly,
                IsVerify: false,
                IsHidden: firstEntry.IsSystem,
                IsBinary: true,
                IsBasic: false,
                IsAscii: false
            );

            yield return new FileEntry(
                FileName: firstEntry.FullFileName,
                Extension: firstEntry.Extension.TrimEnd(),
                Mode: mode,
                Attributes: attributes,
                Size: (int)totalSize,
                LoadAddress: 0,
                ExecuteAddress: 0,
                ModifiedDate: DateTime.MinValue,
                IsProtected: firstEntry.IsReadOnly
            );
        }
    }

    /// <inheritdoc/>
    public bool FileExists(string fileName)
    {
        var (name, extension) = CpmFileNameValidator.NormalizeFileName(fileName);
        var cpmEntries = GetCpmDirectoryEntries();
        
        return cpmEntries.Any(e => 
            e.IsValid && 
            e.FileName.TrimEnd() == name && 
            e.Extension.TrimEnd() == extension);
    }

    /// <inheritdoc/>
    public long GetFileSize(string fileName)
    {
        var (name, extension) = CpmFileNameValidator.NormalizeFileName(fileName);
        var cpmEntries = GetCpmDirectoryEntries();
        
        var fileEntries = cpmEntries
            .Where(e => e.IsValid && 
                       e.FileName.TrimEnd() == name && 
                       e.Extension.TrimEnd() == extension)
            .OrderBy(e => e.ExtentNumber)
            .ToList();

        if (fileEntries.Count == 0)
            throw new Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException($"File not found: {fileName}");

        long totalSize = 0;
        foreach (var entry in fileEntries)
        {
            if (entry == fileEntries.Last())
            {
                totalSize += entry.RecordCount * 128L;
            }
            else
            {
                totalSize += 16384;
            }
        }

        return totalSize;
    }

    /// <inheritdoc/>
    public void ExportFile(string fileName, string destinationPath)
    {
        var (name, extension) = CpmFileNameValidator.NormalizeFileName(fileName);
        var cpmEntries = GetCpmDirectoryEntries();
        
        var fileEntries = cpmEntries
            .Where(e => e.IsValid && 
                       e.FileName.TrimEnd() == name && 
                       e.Extension.TrimEnd() == extension)
            .OrderBy(e => e.ExtentNumber)
            .ToList();

        if (fileEntries.Count == 0)
            throw new Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException($"File not found: {fileName}");

        using var output = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
        
        // Calculate total file size from extents
        long totalFileSize = 0;
        foreach (var entry in fileEntries)
        {
            if (entry == fileEntries.Last())
            {
                totalFileSize += entry.RecordCount * 128L;
            }
            else
            {
                totalFileSize += 16384; // Full extent = 128 records * 128 bytes
            }
        }
        
        // Allocate buffer for the entire file
        var dataArray = new byte[totalFileSize];
        int dataIndex = 0;
        
        foreach (var entry in fileEntries)
        {
            var recordsToRead = entry == fileEntries.Last() ? entry.RecordCount : 128;
            
            for (int record = 0; record < recordsToRead; record++)
            {
                var data = ReadRecord(entry, record);
                if (data != null)
                {
                    Array.Copy(data, 0, dataArray, dataIndex, Math.Min(data.Length, 128));
                    dataIndex += 128;
                }
            }
        }
        
        // Find actual content end
        int actualFileSize = dataIndex; // Default to actual data written
        
        // First, look for CP/M EOF marker (Ctrl+Z = 0x1A)
        bool foundEofMarker = false;
        for (int i = 0; i < dataIndex; i++)
        {
            if (dataArray[i] == 0x1A)
            {
                actualFileSize = i;
                foundEofMarker = true;
                break;
            }
        }
        
        // If no EOF marker found, trim trailing null bytes
        if (!foundEofMarker)
        {
            // Find the last non-null byte within the actual data range
            for (int i = dataIndex - 1; i >= 0; i--)
            {
                if (dataArray[i] != 0)
                {
                    actualFileSize = i + 1;
                    break;
                }
            }
        }
        
        // Write only the actual content (excluding EOF marker and padding)
        if (actualFileSize > 0)
        {
            output.Write(dataArray, 0, actualFileSize);
        }
    }

    /// <inheritdoc/>
    public void ImportFile(string sourcePath, string destinationFileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Disk container is read-only");

        // Normalize the filename first, then validate
        var (tempName, tempExtension) = CpmFileNameValidator.NormalizeFileName(destinationFileName);
        var normalizedFileName = string.IsNullOrEmpty(tempExtension) ? tempName : $"{tempName}.{tempExtension}";
        
        if (!CpmFileNameValidator.IsValidFileName(normalizedFileName))
            throw new ArgumentException($"Invalid CP/M filename: {destinationFileName}");

        var (name, extension) = CpmFileNameValidator.FormatForDirectory(destinationFileName);
        
        // Check if file already exists
        if (FileExists(destinationFileName))
            throw new InvalidOperationException($"File already exists: {destinationFileName}");

        var fileInfo = new FileInfo(sourcePath);
        if (!fileInfo.Exists)
            throw new System.IO.FileNotFoundException($"Source file not found: {sourcePath}");

        // Calculate required extents
        var fileSize = fileInfo.Length;
        var requiredExtents = (int)((fileSize + 16383) / 16384);
        
        // Find free directory entries
        var freeEntries = FindFreeDirectoryEntries(requiredExtents);
        if (freeEntries.Count < requiredExtents)
            throw new InvalidOperationException("Not enough free directory entries");

        // Allocate blocks for the file
        var blocks = AllocateBlocks((int)((fileSize + _config.BlockSize - 1) / _config.BlockSize));
        if (blocks.Count == 0)
            throw new InvalidOperationException("Not enough free space on disk");

        // Write file data with EOF marker only if file doesn't end on record boundary
        using var input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[_config.BlockSize];
        var blockIndex = 0;
        var totalBytesWritten = 0L;
        var originalFileSize = fileInfo.Length;
        var needsEofMarker = (originalFileSize % 128) != 0; // Add EOF if file doesn't end on record boundary

        foreach (var block in blocks)
        {
            var isLastBlock = (blockIndex == blocks.Count - 1);
            var bytesRead = input.Read(buffer, 0, buffer.Length);
            
            if (bytesRead > 0)
            {
                // If this is the last block and we need an EOF marker,
                // add CP/M EOF marker (Ctrl+Z = 0x1A) after the actual data
                if (isLastBlock && needsEofMarker && input.Position >= originalFileSize && bytesRead < buffer.Length)
                {
                    buffer[bytesRead] = 0x1A; // CP/M EOF marker
                    bytesRead++; // Include the EOF marker in the written data
                }
                
                WriteBlock(block, buffer, bytesRead);
                totalBytesWritten += bytesRead;
            }
            blockIndex++;
        }

        // Create directory entries
        var blocksPerExtent = 16384 / _config.BlockSize;
        for (int extent = 0; extent < requiredExtents; extent++)
        {
            var entry = new CpmFileEntry
            {
                UserNumber = 0,
                FileName = name,
                Extension = extension,
                ExtentLow = (byte)(extent & 0xFF),
                ExtentHigh = (byte)((extent >> 8) & 0xFF),
                RecordCount = (byte)(extent < requiredExtents - 1 ? 128 : 
                    Math.Min(128, (fileSize - extent * 16384 + 127) / 128)),
                AllocationBlocks = blocks
                    .Skip(extent * blocksPerExtent)
                    .Take(blocksPerExtent)
                    .Select(b => (ushort)b)
                    .ToArray(),
                IsReadOnly = false,
                IsSystem = false,
                IsArchived = false
            };

            WriteDirectoryEntry(freeEntries[extent], entry);
        }

        InvalidateDirectoryCache();
    }

    /// <inheritdoc/>
    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Disk container is read-only");

        var (name, extension) = CpmFileNameValidator.NormalizeFileName(fileName);
        var cpmEntries = GetCpmDirectoryEntries();
        
        var fileEntries = cpmEntries
            .Where(e => e.IsValid && 
                       e.FileName.TrimEnd() == name && 
                       e.Extension.TrimEnd() == extension)
            .ToList();

        if (fileEntries.Count == 0)
            throw new Legacy89DiskKit.FileSystem.Domain.Exception.FileNotFoundException($"File not found: {fileName}");

        // Mark all extents as deleted
        foreach (var entry in fileEntries)
        {
            var deletedEntry = entry with { UserNumber = CpmFileEntry.DeletedMarker };
            WriteDirectoryEntry(GetDirectoryIndex(entry), deletedEntry);
        }

        InvalidateDirectoryCache();
    }

    /// <inheritdoc/>
    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Disk container is read-only");

        // Clear directory area
        var emptySector = new byte[_config.SectorSize];
        Array.Fill(emptySector, CpmFileEntry.EmptyMarker);

        var directorySectors = (_config.DirectoryEntries * CpmFileEntry.EntrySize) / _config.SectorSize;
        var track = _config.DirectoryStartTrack;
        var sector = 1;

        for (int i = 0; i < directorySectors; i++)
        {
            _diskContainer.WriteSector(track, 0, sector, emptySector);
            
            sector++;
            if (sector > _config.SectorsPerTrack)
            {
                sector = 1;
                track++;
            }
        }

        InvalidateDirectoryCache();
    }

    /// <inheritdoc/>
    public string GetVolumeLabel() => "CP/M DISK";

    /// <inheritdoc/>
    public void SetVolumeLabel(string label)
    {
        // CP/M doesn't support volume labels in the standard format
        // Some implementations use a special directory entry, but we'll skip this
    }

    /// <inheritdoc/>
    public void CreateDirectory(string directoryName)
    {
        throw new NotSupportedException("CP/M does not support directories");
    }

    /// <inheritdoc/>
    public void DeleteDirectory(string directoryName)
    {
        throw new NotSupportedException("CP/M does not support directories");
    }

    /// <inheritdoc/>
    public IEnumerable<FileEntry> GetFiles(string pattern)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            CpmFileNameValidator.WildcardToRegex(pattern),
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
        var (name, extension) = CpmFileNameValidator.NormalizeFileName(fileName);
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
        var tempPath = Path.GetTempFileName();
        try
        {
            ExportFile(fileName, tempPath);
            return File.ReadAllBytes(tempPath);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <inheritdoc/>
    public void WriteFile(string fileName, byte[] data, bool isText = false, ushort loadAddress = 0, ushort execAddress = 0)
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempPath, data);
            ImportFile(tempPath, fileName);
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
        // CP/M doesn't have a standard boot sector format like FAT
        return new BootSector(
            IsBootable: false,
            Label: "CP/M DISK",
            Extension: "",
            Size: 0,
            LoadAddress: 0,
            ExecuteAddress: 0,
            ModifiedDate: DateTime.MinValue,
            StartSector: 0
        );
    }

    /// <inheritdoc/>
    public void WriteBootSector(BootSector bootSector)
    {
        // CP/M doesn't support boot sector modification in this implementation
        throw new NotSupportedException("CP/M does not support boot sector modification");
    }

    /// <inheritdoc/>
    public HuBasicFileSystemInfo GetFileSystemInfo()
    {
        var allocatedBlocks = new HashSet<int>();
        var entries = GetCpmDirectoryEntries();
        
        foreach (var entry in entries.Where(e => e.IsValid))
        {
            foreach (var block in entry.AllocationBlocks.Where(b => b > 0))
            {
                allocatedBlocks.Add(block);
            }
        }

        var totalBlocks = _config.TotalBlocks;
        var freeBlocks = totalBlocks - allocatedBlocks.Count;

        return new HuBasicFileSystemInfo(
            TotalClusters: totalBlocks,
            FreeClusters: freeBlocks,
            ClusterSize: _config.BlockSize,
            SectorSize: _config.SectorSize
        );
    }

    #region Private Methods

    private List<CpmFileEntry> GetCpmDirectoryEntries()
    {
        if (_directoryCacheValid)
            return _directoryCache;

        _directoryCache.Clear();
        
        var entriesPerSector = _config.SectorSize / CpmFileEntry.EntrySize;
        var totalSectors = (_config.DirectoryEntries + entriesPerSector - 1) / entriesPerSector;
        
        var track = _config.DirectoryStartTrack;
        var sector = 1;

        for (int i = 0; i < totalSectors; i++)
        {
            var sectorData = _diskContainer.ReadSector(track, 0, sector);
            
            for (int j = 0; j < entriesPerSector && _directoryCache.Count < _config.DirectoryEntries; j++)
            {
                var offset = j * CpmFileEntry.EntrySize;
                var entry = ParseDirectoryEntry(sectorData, offset);
                _directoryCache.Add(entry);
            }

            sector++;
            if (sector > _config.SectorsPerTrack)
            {
                sector = 1;
                track++;
            }
        }

        _directoryCacheValid = true;
        return _directoryCache;
    }

    private CpmFileEntry ParseDirectoryEntry(byte[] data, int offset)
    {
        var userNumber = data[offset];
        var fileName = Encoding.ASCII.GetString(data, offset + 1, 8);
        var extension = Encoding.ASCII.GetString(data, offset + 9, 3);
        var extentLow = data[offset + 12];
        var extentHigh = data[offset + 14];
        var recordCount = data[offset + 15];

        // Read allocation blocks (8 or 16 depending on disk size)
        var blockCount = _config.TotalBlocks < 256 ? 16 : 8;
        var blocks = new ushort[blockCount];
        
        if (_config.TotalBlocks < 256)
        {
            // 8-bit block numbers
            for (int i = 0; i < 16; i++)
            {
                blocks[i] = data[offset + 16 + i];
            }
        }
        else
        {
            // 16-bit block numbers
            for (int i = 0; i < 8; i++)
            {
                blocks[i] = (ushort)(data[offset + 16 + i * 2] | 
                                   (data[offset + 17 + i * 2] << 8));
            }
        }

        // Extract file attributes from high bits
        bool isReadOnly = (data[offset + 9] & 0x80) != 0;
        bool isSystem = (data[offset + 10] & 0x80) != 0;
        bool isArchived = (data[offset + 11] & 0x80) != 0;

        // Clear attribute bits from name/extension
        var cleanExtension = new byte[3];
        cleanExtension[0] = (byte)(data[offset + 9] & 0x7F);
        cleanExtension[1] = (byte)(data[offset + 10] & 0x7F);
        cleanExtension[2] = (byte)(data[offset + 11] & 0x7F);
        extension = Encoding.ASCII.GetString(cleanExtension);

        return new CpmFileEntry
        {
            UserNumber = userNumber,
            FileName = fileName,
            Extension = extension,
            ExtentLow = extentLow,
            ExtentHigh = extentHigh,
            RecordCount = recordCount,
            AllocationBlocks = blocks,
            IsReadOnly = isReadOnly,
            IsSystem = isSystem,
            IsArchived = isArchived
        };
    }

    private void WriteDirectoryEntry(int index, CpmFileEntry entry)
    {
        var entriesPerSector = _config.SectorSize / CpmFileEntry.EntrySize;
        var sectorIndex = index / entriesPerSector;
        var entryOffset = (index % entriesPerSector) * CpmFileEntry.EntrySize;
        
        var track = _config.DirectoryStartTrack;
        var sector = 1 + sectorIndex;
        
        while (sector > _config.SectorsPerTrack)
        {
            sector -= _config.SectorsPerTrack;
            track++;
        }

        // Read current sector
        var sectorData = _diskContainer.ReadSector(track, 0, sector);
        
        // Update entry
        sectorData[entryOffset] = entry.UserNumber;
        
        var nameBytes = Encoding.ASCII.GetBytes(entry.FileName.PadRight(8));
        Array.Copy(nameBytes, 0, sectorData, entryOffset + 1, 8);
        
        var extBytes = Encoding.ASCII.GetBytes(entry.Extension.PadRight(3));
        Array.Copy(extBytes, 0, sectorData, entryOffset + 9, 3);
        
        // Set attribute bits
        if (entry.IsReadOnly) sectorData[entryOffset + 9] |= 0x80;
        if (entry.IsSystem) sectorData[entryOffset + 10] |= 0x80;
        if (entry.IsArchived) sectorData[entryOffset + 11] |= 0x80;
        
        sectorData[entryOffset + 12] = entry.ExtentLow;
        sectorData[entryOffset + 13] = 0; // S1 (reserved)
        sectorData[entryOffset + 14] = entry.ExtentHigh;
        sectorData[entryOffset + 15] = entry.RecordCount;
        
        // Write allocation blocks
        if (_config.TotalBlocks < 256)
        {
            for (int i = 0; i < 16; i++)
            {
                sectorData[entryOffset + 16 + i] = (byte)(i < entry.AllocationBlocks.Length ? 
                    entry.AllocationBlocks[i] : 0);
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                if (i < entry.AllocationBlocks.Length)
                {
                    sectorData[entryOffset + 16 + i * 2] = (byte)(entry.AllocationBlocks[i] & 0xFF);
                    sectorData[entryOffset + 17 + i * 2] = (byte)(entry.AllocationBlocks[i] >> 8);
                }
                else
                {
                    sectorData[entryOffset + 16 + i * 2] = 0;
                    sectorData[entryOffset + 17 + i * 2] = 0;
                }
            }
        }
        
        // Write back sector
        _diskContainer.WriteSector(track, 0, sector, sectorData);
    }

    private byte[]? ReadRecord(CpmFileEntry entry, int recordIndex)
    {
        if (recordIndex >= 128)
            return null;

        var recordsPerBlock = _config.BlockSize / 128;
        var blockIndex = recordIndex / recordsPerBlock;
        var recordInBlock = recordIndex % recordsPerBlock;

        if (blockIndex >= entry.AllocationBlocks.Length || 
            entry.AllocationBlocks[blockIndex] == 0)
            return null;

        var blockNumber = entry.AllocationBlocks[blockIndex];
        var blockData = ReadBlock(blockNumber);
        
        var result = new byte[128];
        var sourceOffset = recordInBlock * 128;
        var copyLength = Math.Min(128, blockData.Length - sourceOffset);
        if (copyLength > 0)
        {
            Array.Copy(blockData, sourceOffset, result, 0, copyLength);
        }
        
        return result;
    }

    private byte[] ReadBlock(int blockNumber)
    {
        if (blockNumber == 0 || blockNumber > _config.TotalBlocks)
            throw new ArgumentException($"Invalid block number: {blockNumber}");

        var result = new byte[_config.BlockSize];
        var sectorsPerBlock = _config.BlockSize / _config.SectorSize;
        
        // In CP/M, block numbering is straightforward:
        // Block 0 is reserved, blocks start from physical sector after directory
        var directorySize = _config.DirectoryEntries * CpmFileEntry.EntrySize;
        var directorySectors = (directorySize + _config.SectorSize - 1) / _config.SectorSize;
        var reservedSectors = _config.ReservedTracks * _config.SectorsPerTrack;
        
        // First data sector is after reserved tracks and directory
        var firstDataSector = reservedSectors + directorySectors;
        
        // Calculate absolute sector for this block
        var firstSectorOfBlock = firstDataSector + (blockNumber - 1) * sectorsPerBlock;
        
        for (int i = 0; i < sectorsPerBlock; i++)
        {
            var absoluteSector = firstSectorOfBlock + i;
            var track = absoluteSector / _config.SectorsPerTrack;
            var sector = (absoluteSector % _config.SectorsPerTrack) + 1;
            
            var sectorData = _diskContainer.ReadSector(track, 0, sector);
            Array.Copy(sectorData, 0, result, i * _config.SectorSize, _config.SectorSize);
        }
        
        return result;
    }

    private void WriteBlock(int blockNumber, byte[] data, int dataLength)
    {
        if (blockNumber == 0 || blockNumber > _config.TotalBlocks)
            throw new ArgumentException($"Invalid block number: {blockNumber}");

        var sectorsPerBlock = _config.BlockSize / _config.SectorSize;
        
        // In CP/M, block numbering is straightforward:
        // Block 0 is reserved, blocks start from physical sector after directory
        var directorySize = _config.DirectoryEntries * CpmFileEntry.EntrySize;
        var directorySectors = (directorySize + _config.SectorSize - 1) / _config.SectorSize;
        var reservedSectors = _config.ReservedTracks * _config.SectorsPerTrack;
        
        // First data sector is after reserved tracks and directory
        var firstDataSector = reservedSectors + directorySectors;
        
        // Calculate absolute sector for this block
        var firstSectorOfBlock = firstDataSector + (blockNumber - 1) * sectorsPerBlock;
        
        for (int i = 0; i < sectorsPerBlock; i++)
        {
            var absoluteSector = firstSectorOfBlock + i;
            var track = absoluteSector / _config.SectorsPerTrack;
            var sector = (absoluteSector % _config.SectorsPerTrack) + 1;
            
            var sectorData = new byte[_config.SectorSize];
            var offset = i * _config.SectorSize;
            var copyLength = Math.Min(_config.SectorSize, dataLength - offset);
            
            if (copyLength > 0)
            {
                Array.Copy(data, offset, sectorData, 0, copyLength);
            }
            
            _diskContainer.WriteSector(track, 0, sector, sectorData);
        }
    }

    private List<int> FindFreeDirectoryEntries(int count)
    {
        var entries = GetCpmDirectoryEntries();
        var freeIndices = new List<int>();
        
        for (int i = 0; i < entries.Count && freeIndices.Count < count; i++)
        {
            if (!entries[i].IsValid)
            {
                freeIndices.Add(i);
            }
        }
        
        return freeIndices;
    }

    private List<int> AllocateBlocks(int count)
    {
        var allocatedBlocks = new HashSet<int>();
        var entries = GetCpmDirectoryEntries();
        
        // Collect all used blocks
        foreach (var entry in entries.Where(e => e.IsValid && !string.IsNullOrWhiteSpace(e.FileName.Trim())))
        {
            foreach (var block in entry.AllocationBlocks.Where(b => b > 0))
            {
                allocatedBlocks.Add(block);
            }
        }
        
        // Find free blocks (starting from block 1, block 0 is reserved)
        var freeBlocks = new List<int>();
        for (int block = 1; block <= _config.TotalBlocks && freeBlocks.Count < count; block++)
        {
            if (!allocatedBlocks.Contains(block))
            {
                freeBlocks.Add(block);
            }
        }
        
        return freeBlocks;
    }

    private int GetDirectoryIndex(CpmFileEntry entry)
    {
        var entries = GetCpmDirectoryEntries();
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] == entry)
                return i;
        }
        return -1;
    }

    private void InvalidateDirectoryCache()
    {
        _directoryCacheValid = false;
    }

    private string BuildAttributes(CpmFileEntry entry)
    {
        var attrs = new List<string>();
        if (entry.IsReadOnly) attrs.Add("R");
        if (entry.IsSystem) attrs.Add("S");
        if (entry.IsArchived) attrs.Add("A");
        return string.Join("", attrs);
    }

    #endregion
}