using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Legacy89DiskKit.Domain.FileSystem.Exception;
using DomainAttr = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public class HuBasicFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly HuBasicConfiguration _config;
    private readonly HuBasicFatManager _fatManager;
    private readonly HuBasicDirParser _dirParser;

    public HuBasicFileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer;
        _config = HuBasicConfiguration.GetDefault(diskContainer.DiskType);
        _fatManager = new HuBasicFatManager(diskContainer, _config);
        _dirParser = new HuBasicDirParser(_config);
    }

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        var fat = _fatManager.ReadFat();
        int free = 0;
        int maxIndex = _diskContainer.DiskType == DiskType.TwoHD ? 512 : _config.TotalClusters;
        for (int i = _config.ReservedClusters; i < maxIndex; i++)
        {
            if (_diskContainer.DiskType == DiskType.TwoHD && (i % 256) >= 0x80) continue;
            if (_fatManager.GetFatEntry(fat, i) == 0x00) free++;
        }

        return new DiskFileSystemInfo(
            "Hu-BASIC",
            (long)_config.TotalClusters * _config.ClusterSize,
            (long)free * _config.ClusterSize,
            _config.ClusterSize,
            _config.ReservedClusters * (_config.ClusterSize / _config.SectorSize),
            "X1"
        );
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        var files = new List<FileEntry>();
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            for (int offset = 0; offset < _config.SectorSize; offset += 32)
            {
                byte mode = dirData[offset];
                if (mode == 0xFF) return files;
                if (mode == 0x00 || mode == 0xE5) continue;

                var entryData = new byte[32];
                Array.Copy(dirData, offset, entryData, 0, 32);
                var entry = _dirParser.Parse(entryData);

                files.Add(entry);
            }
        }
        return files;
    }

    public byte[] ReadFile(string fileName)
    {
        var file = GetFiles().FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (file == null) throw new FileSystemException($"File not found: {fileName}");

        var (clusters, terminalFlag) = _fatManager.GetClusterChainWithTerminal(file.StartCluster);
        using var ms = new MemoryStream();
        foreach (var c in clusters)
        {
            ms.Write(ReadCluster(c));
        }

        var data = ms.ToArray();
        
        // Determine actual size from FAT terminal flag if it's a 2HD disk and size is 0 (common for some binary dumps)
        // Or if the size field is used but the FAT specifies the record count.
        int recordCount = clusters.Count * (_config.ClusterSize / _config.SectorSize);
        if (_diskContainer.DiskType == DiskType.TwoHD && terminalFlag >= 0x80 && terminalFlag <= 0x8F)
        {
            int usedInLast = terminalFlag - 0x7F;
            int totalRecords = (clusters.Count - 1) * (_config.ClusterSize / _config.SectorSize) + usedInLast;
            int totalBytes = totalRecords * _config.SectorSize;
            
            if (file.Size == 0 || totalBytes < data.Length)
            {
                data = data.Take(totalBytes).ToArray();
            }
        }

        if (file.Attributes.IsAscii)
        {
            return ExtractAscii(data);
        }
        return (file.Size > 0 && data.Length > file.Size) ? data.Take((int)file.Size).ToArray() : data;
    }

    public bool FileExists(string fileName)
    {
        return GetFiles().Any(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot write to read-only disk");

        if (FileExists(fileName))
            throw new FileSystemException($"File already exists: {fileName}");

        // Hu-BASIC ASCII files must end with 0x1A
        if (attributes.IsAscii && (data.Length == 0 || data[^1] != 0x1A))
        {
            var newData = new byte[data.Length + 1];
            Array.Copy(data, newData, data.Length);
            newData[^1] = 0x1A;
            data = newData;
        }

        int clustersNeeded = (data.Length + _config.ClusterSize - 1) / _config.ClusterSize;
        if (clustersNeeded == 0) clustersNeeded = 1;

        var allocatedClusters = AllocateClusters(clustersNeeded);
        if (allocatedClusters.Count < clustersNeeded)
            throw new FileSystemException("Not enough free space");

        WriteDataToClusters(data, allocatedClusters);

        // Update FAT
        var fat = _fatManager.ReadFat();
        int sectorsInLastCluster = ((data.Length + _config.SectorSize - 1) / _config.SectorSize) % (_config.ClusterSize / _config.SectorSize);
        if (sectorsInLastCluster == 0) sectorsInLastCluster = _config.ClusterSize / _config.SectorSize;
        int terminalFlag = 0x7F + sectorsInLastCluster;

        for (int i = 0; i < allocatedClusters.Count; i++)
        {
            int next = (i == allocatedClusters.Count - 1) ? terminalFlag : allocatedClusters[i + 1];
            _fatManager.SetFatEntry(fat, allocatedClusters[i], next);
        }
        _fatManager.WriteFat(fat);

        // Create and add directory entry
        var (name, ext) = ParseFileName(fileName);
        var entry = new FileEntry(name, ext, data.Length, null, DateTime.Now, attributes, allocatedClusters[0], loadAddress, (ushort?)(loadAddress + data.Length - 1), executionAddress);
        AddDirectoryEntry(entry);
    }

    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot delete from read-only disk");

        var file = GetFiles().FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (file == null) throw new FileSystemException($"File not found: {fileName}");

        var clusters = _fatManager.GetClusterChain(file.StartCluster);
        FreeClusters(clusters);
        MarkDirectoryEntryAsDeleted(fileName);
    }

    public FileSystemCapabilities Capabilities => 
        FileSystemCapabilities.SupportsAttributes | 
        FileSystemCapabilities.SupportsRename | 
        FileSystemCapabilities.FixedFileNameLength;

    public void RenameFile(string oldName, string newName)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        
        // Find entry
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            bool modified = false;

            for (int offset = 0; offset < _config.SectorSize; offset += 32)
            {
                byte mode = dirData[offset];
                if (mode == 0xFF) break;
                if (mode == 0x00) continue;

                var entryData = new byte[32];
                Array.Copy(dirData, offset, entryData, 0, 32);
                var entry = _dirParser.Parse(entryData);

                if (entry.FullName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                {
                    var (name, ext) = ParseFileName(newName);
                    var updatedEntry = entry with { FileName = name, Extension = ext };
                    _dirParser.WriteToBuffer(dirData, offset, updatedEntry);
                    modified = true;
                    break;
                }
            }

            if (modified)
            {
                WriteDirectorySector(s, dirData);
                return;
            }
        }
        throw new FileSystemException($"File not found: {oldName}");
    }

    public void CopyFile(string sourceName, string targetName)
    {
        var data = ReadFile(sourceName);
        var sourceFile = GetFiles().First(f => f.FullName.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
        WriteFile(targetName, data, sourceFile.Attributes);
    }

    public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            bool modified = false;

            for (int offset = 0; offset < _config.SectorSize; offset += 32)
            {
                byte mode = dirData[offset];
                if (mode == 0xFF) break;
                if (mode == 0x00) continue;

                var entryData = new byte[32];
                Array.Copy(dirData, offset, entryData, 0, 32);
                var entry = _dirParser.Parse(entryData);

                if (entry.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    var updatedEntry = entry with { Attributes = attributes };
                    _dirParser.WriteToBuffer(dirData, offset, updatedEntry);
                    modified = true;
                    break;
                }
            }

            if (modified)
            {
                WriteDirectorySector(s, dirData);
                return;
            }
        }
    }

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii)
    {
        return new ExtendedFileAttributes(
            DomainAttr.None, 
            (byte)(isAscii ? 0x04 : 0x01), // bit 2 for ASC, bit 0 for BIN
            true, 
            "");
    }

    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot format read-only disk");

        // Initialize FAT with all free (0x00) except reserved clusters
        var fatData = new byte[_config.FatSectors * _config.SectorSize];
        for (int i = 0; i < _config.ReservedClusters; i++)
        {
            // Standard Hu-BASIC uses 0x01, 0x02... 0x8F for reserved tracks
            int next = (i == _config.ReservedClusters - 1) ? 0x8F : i + 1;
            _fatManager.SetFatEntry(fatData, i, (ushort)next);
        }
        _fatManager.WriteFat(fatData);

        // Initialize Directory with 0xFF (End of Directory)
        var emptySector = new byte[_config.SectorSize];
        Array.Fill(emptySector, (byte)0xFF);
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            WriteDirectorySector(s, emptySector);
        }
    }

    public byte[] ReadBootArea() 
    {
        return _diskContainer.ReadSector(0, 0, 1);
    }

    public void WriteBootArea(byte[] data)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot write to read-only disk");
        
        var sectorData = new byte[_config.SectorSize];
        Array.Copy(data, sectorData, Math.Min(data.Length, _config.SectorSize));
        _diskContainer.WriteSector(0, 0, 1, sectorData);
    }

    private void FreeClusters(List<int> clusters)
    {
        var fatData = _fatManager.ReadFat();
        foreach (var cluster in clusters)
        {
            _fatManager.SetFatEntry(fatData, cluster, 0x00);
        }
        _fatManager.WriteFat(fatData);
    }

    private void MarkDirectoryEntryAsDeleted(string fileName)
    {
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            bool modified = false;

            for (int offset = 0; offset < _config.SectorSize; offset += 32)
            {
                byte mode = dirData[offset];
                if (mode == 0xFF) return;
                if (mode == 0x00) continue;

                var entryData = new byte[32];
                Array.Copy(dirData, offset, entryData, 0, 32);
                var entry = _dirParser.Parse(entryData);

                if (entry.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    dirData[offset] = 0x00; // Deleted
                    modified = true;
                    break;
                }
            }

            if (modified)
            {
                WriteDirectorySector(s, dirData);
                return;
            }
        }
    }

    private List<int> AllocateClusters(int count)
    {
        var fat = _fatManager.ReadFat();
        var allocated = new List<int>();
        int maxIndex = _diskContainer.DiskType == DiskType.TwoHD ? 512 : _config.TotalClusters;

        for (int i = _config.ReservedClusters; i < maxIndex && allocated.Count < count; i++)
        {
            if (_diskContainer.DiskType == DiskType.TwoHD && (i % 256) >= 0x80) continue;

            var entry = _fatManager.GetFatEntry(fat, i);
            if (entry == 0) allocated.Add(i);
        }
        return allocated;
    }

    private void WriteDataToClusters(byte[] data, List<int> clusters)
    {
        int offset = 0;
        foreach (var cluster in clusters)
        {
            var clusterData = new byte[_config.ClusterSize];
            int toCopy = Math.Min(data.Length - offset, _config.ClusterSize);
            Array.Copy(data, offset, clusterData, 0, toCopy);
            WriteCluster(cluster, clusterData);
            offset += toCopy;
        }
    }

    private void WriteCluster(int cluster, byte[] data)
    {
        int sectorsPerCluster = _config.ClusterSize / _config.SectorSize;
        int startRecord = cluster * sectorsPerCluster;
        for (int i = 0; i < sectorsPerCluster; i++)
        {
            var (c, h, s) = GetPhysicalAddressFromRecord(startRecord + i);
            var sectorData = new byte[_config.SectorSize];
            Array.Copy(data, i * _config.SectorSize, sectorData, 0, _config.SectorSize);
            _diskContainer.WriteSector(c, h, s, sectorData);
        }
    }

    private void AddDirectoryEntry(FileEntry entry)
    {
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            for (int offset = 0; offset < _config.SectorSize; offset += 32)
            {
                byte mode = dirData[offset];
                if (mode == 0x00 || mode == 0xFF)
                {
                    _dirParser.WriteToBuffer(dirData, offset, entry);
                    WriteDirectorySector(s, dirData);
                    return;
                }
            }
        }
        throw new FileSystemException("Directory is full");
    }

    private (string name, string ext) ParseFileName(string fileName)
    {
        var parts = fileName.Split('.');
        string name = parts[0];
        if (name.Length > 13) name = name.Substring(0, 13);
        string ext = parts.Length > 1 ? parts[1] : "";
        if (ext.Length > 3) ext = ext.Substring(0, 3);
        return (name, ext);
    }

    private byte[] ReadDirectorySector(int sectorIndex)
    {
        int recordNumber = (_config.DirectoryTrack * _config.SectorsPerTrack) + (_config.DirectorySector - 1) + sectorIndex;
        var (c, h, s) = GetPhysicalAddressFromRecord(recordNumber);
        return _diskContainer.ReadSector(c, h, s);
    }

    private void WriteDirectorySector(int sectorIndex, byte[] dirData)
    {
        int recordNumber = (_config.DirectoryTrack * _config.SectorsPerTrack) + (_config.DirectorySector - 1) + sectorIndex;
        var (c, h, s) = GetPhysicalAddressFromRecord(recordNumber);
        _diskContainer.WriteSector(c, h, s, dirData);
    }

    private byte[] ReadCluster(int cluster)
    {
        var sectorsPerCluster = _config.ClusterSize / _config.SectorSize;
        var clusterData = new byte[_config.ClusterSize];
        int startRecord = cluster * sectorsPerCluster;
        
        for (int i = 0; i < sectorsPerCluster; i++)
        {
            var (c, h, s) = GetPhysicalAddressFromRecord(startRecord + i);
            var sectorData = _diskContainer.ReadSector(c, h, s);
            Array.Copy(sectorData, 0, clusterData, i * _config.SectorSize, _config.SectorSize);
        }
        return clusterData;
    }

    private (int cylinder, int head, int sector) GetPhysicalAddressFromRecord(int recordNumber)
    {
        int cylinder = (recordNumber / _config.SectorsPerTrack) / 2;
        int head = (recordNumber / _config.SectorsPerTrack) % 2;
        int sectorNum = (recordNumber % _config.SectorsPerTrack) + 1;
        return (cylinder, head, sectorNum);
    }

    private byte[] ExtractAscii(byte[] data)
    {
        var res = new List<byte>();
        foreach (var b in data)
        {
            if (b == 0x1A) break;
            res.Add(b);
        }
        return res.ToArray();
    }

    public void Dispose() { }
}
