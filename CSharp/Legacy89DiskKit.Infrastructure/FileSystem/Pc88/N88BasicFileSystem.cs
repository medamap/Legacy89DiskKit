using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Models;
using Legacy89DiskKit.Domain.FileSystem.Exception;
using DomainAttr = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.Pc88;

public class N88BasicFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly N88BasicConfiguration _config;
    private readonly N88BasicFatManager _fatManager;
    private readonly N88BasicDirParser _dirParser;

    public N88BasicFileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer;
        _config = N88BasicConfiguration.GetDefault(diskContainer.DiskType);
        _fatManager = new N88BasicFatManager(diskContainer, _config);
        _dirParser = new N88BasicDirParser(_config);
    }

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        var fat = _fatManager.ReadFat();
        int free = 0;
        for (int i = _config.ReservedClusters; i < _config.TotalClusters; i++)
        {
            if (_fatManager.GetFatEntry(fat, i) == 0xFF) free++;
        }

        return new DiskFileSystemInfo(
            "N88-BASIC",
            (long)_config.TotalClusters * _config.ClusterSize,
            (long)free * _config.ClusterSize,
            _config.ClusterSize,
            _config.ReservedClusters * (_config.ClusterSize / _config.SectorSize),
            "PC88",
            "PC88",
            6,
            3
        );
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        var files = new List<FileEntry>();
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            for (int offset = 0; offset < _config.SectorSize; offset += 16)
            {
                byte mode = dirData[offset];
                if (mode == 0xFF) return files;
                if (mode == 0x00) continue;

                var entryData = new byte[16];
                Array.Copy(dirData, offset, entryData, 0, 16);
                var entry = _dirParser.Parse(entryData);

                // Enrich size from FAT chain
                var clusters = _fatManager.GetClusterChain(entry.StartCluster);
                long size = clusters.Count * _config.ClusterSize;
                
                // If it's the last cluster, check for partial sector usage if N88-BASIC supports it in FAT
                var fat = _fatManager.ReadFat();
                if (clusters.Count > 0)
                {
                    byte lastEntry = _fatManager.GetFatEntry(fat, clusters[^1]);
                    if (lastEntry >= 0xC0 && lastEntry <= 0xCF)
                    {
                        int usedSectors = lastEntry - 0xC0;
                        size = (clusters.Count - 1) * _config.ClusterSize + (usedSectors * _config.SectorSize);
                    }
                }
                
                entry = entry with { Size = size };
                files.Add(entry);
            }
        }
        return files;
    }

    public bool FileExists(string fileName)
    {
        return GetFiles().Any(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    public byte[] ReadFile(string fileName)
    {
        var file = GetFiles().FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (file == null) throw new FileSystemException($"File not found: {fileName}");

        var clusters = _fatManager.GetClusterChain(file.StartCluster);
        using var ms = new MemoryStream();
        foreach (var c in clusters)
        {
            ms.Write(ReadCluster(c));
        }

        var data = ms.ToArray();
        // Trim to exact size calculated from FAT
        if (data.Length > file.Size) data = data.Take((int)file.Size).ToArray();

        if (file.Attributes.IsAscii)
        {
            return ExtractAscii(data);
        }
        return data;
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        if (FileExists(fileName)) throw new FileSystemException($"File already exists: {fileName}");

        // N88-BASIC ASCII files often end with 0x1A
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
        if (allocatedClusters.Count < clustersNeeded) throw new FileSystemException("Not enough free space");

        WriteDataToClusters(data, allocatedClusters);

        // Update FAT
        var fat = _fatManager.ReadFat();
        for (int i = 0; i < allocatedClusters.Count; i++)
        {
            int next;
            if (i == allocatedClusters.Count - 1)
            {
                // EOF marker: C0h + sectors used in last cluster
                int lastClusterSize = data.Length % _config.ClusterSize;
                if (lastClusterSize == 0) lastClusterSize = _config.ClusterSize;
                int sectors = (lastClusterSize + _config.SectorSize - 1) / _config.SectorSize;
                next = 0xC0 + sectors;
            }
            else
            {
                next = allocatedClusters[i + 1];
            }
            _fatManager.SetFatEntry(fat, allocatedClusters[i], next);
        }
        _fatManager.WriteFat(fat);

        // Add dir entry
        var (name, ext) = ParseFileName(fileName);
        var entry = new FileEntry(name, ext, data.Length, null, DateTime.Now, attributes, allocatedClusters[0], null, null, null);
        AddDirectoryEntry(entry);
    }

    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        var file = GetFiles().FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (file == null) throw new FileSystemException($"File not found: {fileName}");

        var clusters = _fatManager.GetClusterChain(file.StartCluster);
        var fat = _fatManager.ReadFat();
        foreach (var c in clusters) _fatManager.SetFatEntry(fat, c, 0xFF); // Mark as free
        _fatManager.WriteFat(fat);

        MarkAsDeleted(fileName);
    }

    public void RenameFile(string oldName, string newName)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        UpdateDirEntry(oldName, entry => {
            var (name, ext) = ParseFileName(newName);
            return entry with { FileName = name, Extension = ext };
        });
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
        UpdateDirEntry(fileName, entry => entry with { Attributes = attributes });
    }

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii)
    {
        // bit 0: Binary, bit 7: Tokenized (0 for ASCII)
        byte rawValue = (byte)(isAscii ? 0x00 : 0x01); 
        return new ExtendedFileAttributes(DomainAttr.None, rawValue, isAscii, "PC88");
    }

    public void Format()
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        
        // FAT: all 0xFF
        var fatData = new byte[_config.FatSectors * _config.SectorSize];
        Array.Fill(fatData, (byte)0xFF);
        _fatManager.WriteFat(fatData);

        // Directory: all 0xFF
        var emptySector = new byte[_config.SectorSize];
        Array.Fill(emptySector, (byte)0xFF);
        for (int s = 0; s < _config.DirectorySectors; s++) WriteDirectorySector(s, emptySector);
    }

    public byte[] ReadBootArea() => _diskContainer.ReadSector(0, 0, 1);

    public void WriteBootArea(byte[] data)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        var sectorData = new byte[_config.SectorSize];
        Array.Copy(data, sectorData, Math.Min(data.Length, _config.SectorSize));
        _diskContainer.WriteSector(0, 0, 1, sectorData);
    }

    public FileSystemCapabilities Capabilities => 
        FileSystemCapabilities.SupportsAttributes | 
        FileSystemCapabilities.SupportsRename | 
        FileSystemCapabilities.FixedFileNameLength;

    private byte[] ReadDirectorySector(int sectorIndex)
    {
        var (track, side, sector) = GetPhysicalAddress(_config.SystemTrack, _config.DirectorySector + sectorIndex);
        return _diskContainer.ReadSector(track, side, sector);
    }

    private void WriteDirectorySector(int sectorIndex, byte[] data)
    {
        var (track, side, sector) = GetPhysicalAddress(_config.SystemTrack, _config.DirectorySector + sectorIndex);
        _diskContainer.WriteSector(track, side, sector, data);
    }

    private byte[] ReadCluster(int cluster)
    {
        int sectorsPerCluster = _config.ClusterSize / _config.SectorSize;
        var data = new byte[_config.ClusterSize];
        // In 2D, each cluster is 8 sectors (half track).
        // Cluster 0 = T0/H0/S1-8, Cluster 1 = T0/H0/S9-16, Cluster 2 = T0/H1/S1-8 ...
        int totalSectorIndex = cluster * sectorsPerCluster;
        for (int i = 0; i < sectorsPerCluster; i++)
        {
            int lba = totalSectorIndex + i;
            int track = lba / _config.SectorsPerTrack;
            int sector = (lba % _config.SectorsPerTrack) + 1;
            var (c, h, s) = GetPhysicalAddress(track, sector);
            var sectorData = _diskContainer.ReadSector(c, h, s);
            Array.Copy(sectorData, 0, data, i * _config.SectorSize, _config.SectorSize);
        }
        return data;
    }

    private void WriteCluster(int cluster, byte[] data)
    {
        int sectorsPerCluster = _config.ClusterSize / _config.SectorSize;
        int totalSectorIndex = cluster * sectorsPerCluster;
        for (int i = 0; i < sectorsPerCluster; i++)
        {
            int lba = totalSectorIndex + i;
            int track = lba / _config.SectorsPerTrack;
            int sector = (lba % _config.SectorsPerTrack) + 1;
            var (c, h, s) = GetPhysicalAddress(track, sector);
            var sectorData = new byte[_config.SectorSize];
            Array.Copy(data, i * _config.SectorSize, sectorData, 0, _config.SectorSize);
            _diskContainer.WriteSector(c, h, s, sectorData);
        }
    }

    private (int c, int h, int s) GetPhysicalAddress(int track, int sector)
    {
        if (_diskContainer.DiskType == DiskType.TwoHD)
        {
             // 2HD: 8 sectors/track, 1024 bytes/sector is common but D88 abstracts it.
             // Standard D88 parsing uses linear tracks.
             return (track / 2, track % 2, sector);
        }
        return (track / 2, track % 2, sector);
    }

    private List<int> AllocateClusters(int count)
    {
        var fat = _fatManager.ReadFat();
        var allocated = new List<int>();
        for (int i = 0; i < _config.TotalClusters && allocated.Count < count; i++)
        {
            // Skip system track (clusters overlapping T18/H1 or T40/H0)
            if (IsSystemCluster(i)) continue;
            if (_fatManager.GetFatEntry(fat, i) == 0xFF) allocated.Add(i);
        }
        return allocated;
    }

    private bool IsSystemCluster(int cluster)
    {
        int sectorsPerCluster = _config.ClusterSize / _config.SectorSize;
        int startLba = cluster * sectorsPerCluster;
        int endLba = startLba + sectorsPerCluster - 1;
        int systemLbaStart = _config.SystemTrack * _config.SectorsPerTrack;
        int systemLbaEnd = systemLbaStart + _config.SectorsPerTrack - 1;
        return (startLba <= systemLbaEnd && endLba >= systemLbaStart);
    }

    private void WriteDataToClusters(byte[] data, List<int> clusters)
    {
        int offset = 0;
        foreach (var c in clusters)
        {
            var clusterData = new byte[_config.ClusterSize];
            int toCopy = Math.Min(data.Length - offset, _config.ClusterSize);
            Array.Copy(data, offset, clusterData, 0, toCopy);
            WriteCluster(c, clusterData);
            offset += toCopy;
        }
    }

    private void AddDirectoryEntry(FileEntry entry)
    {
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            for (int offset = 0; offset < _config.SectorSize; offset += 16)
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

    private void MarkAsDeleted(string fileName)
    {
        UpdateDirEntry(fileName, entry => null); // null means mark as 0x00
    }

    private void UpdateDirEntry(string fileName, Func<FileEntry, FileEntry?> updateFunc)
    {
        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            bool modified = false;
            for (int offset = 0; offset < _config.SectorSize; offset += 16)
            {
                byte mode = dirData[offset];
                if (mode == 0xFF) break;
                if (mode == 0x00) continue;
                
                var entryData = new byte[16];
                Array.Copy(dirData, offset, entryData, 0, 16);
                var entry = _dirParser.Parse(entryData);
                if (entry.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    var updated = updateFunc(entry);
                    if (updated == null) dirData[offset] = 0x00;
                    else _dirParser.WriteToBuffer(dirData, offset, updated);
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
        throw new FileSystemException($"File not found: {fileName}");
    }

    private (string name, string ext) ParseFileName(string fileName)
    {
        var parts = fileName.Split('.');
        string name = parts[0].ToUpper();
        if (name.Length > 6) name = name.Substring(0, 6);
        string ext = parts.Length > 1 ? parts[1].ToUpper() : "";
        if (ext.Length > 3) ext = ext.Substring(0, 3);
        return (name, ext);
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
