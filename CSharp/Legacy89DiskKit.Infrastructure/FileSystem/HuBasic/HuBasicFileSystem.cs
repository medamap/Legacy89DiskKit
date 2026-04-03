using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Interface.Layout;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using Legacy89DiskKit.Domain.FileSystem.Exception;
using DomainAttr = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public class HuBasicFileSystem : IFileSystem, IDirectoryLayoutProvider
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
        int maxIndex = HuBasicAllocationRules.GetFatScanLimit(_diskContainer.DiskType, _config);
        for (int i = _config.ReservedClusters; i < maxIndex; i++)
        {
            if (!HuBasicAllocationRules.IsAllocatableCluster(_diskContainer.DiskType, _config, i)) continue;
            if (_fatManager.GetFatEntry(fat, i) == 0x00) free++;
        }

        return new DiskFileSystemInfo(
            "Hu-BASIC",
            (long)_config.TotalClusters * _config.ClusterSize,
            (long)free * _config.ClusterSize,
            _config.ClusterSize,
            _config.ReservedClusters * (_config.ClusterSize / _config.SectorSize),
            "X1",
            "X1",
            8,
            3
        );
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        return GetFilesWithMetadata();
    }

    public IReadOnlyList<FileEntry> GetFilesWithMetadata()
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

        return HuBasicReadRules.ResolveReadPayload(ms.ToArray(), file, _diskContainer.DiskType, _config, clusters.Count, terminalFlag);
    }

    public bool FileExists(string fileName)
    {
        return GetFiles().Any(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
    {
        WriteFileInternal(fileName, data, attributes, loadAddress, executionAddress);
    }

    public void WriteFileInternal(
        string fileName,
        byte[] data,
        ExtendedFileAttributes attributes,
        ushort? loadAddress = null,
        ushort? executionAddress = null,
        byte[]? forcedRawName = null,
        byte[]? forcedRawExtension = null,
        DateTime? forcedModifiedAt = null,
        HuBasicFileMetadata? forcedMetadata = null)
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot write to read-only disk");

        if (FileExists(fileName) && forcedRawName == null)
            throw new FileSystemException($"File already exists: {fileName}");

        if (data.Length > 0xFFFF)
            throw new FileSystemException("Hu-BASIC files larger than 65535 bytes are not supported.");

        data = HuBasicWriteRules.PrepareWritePayload(data, attributes);

        if (data.Length > 0xFFFF)
            throw new FileSystemException("Hu-BASIC files larger than 65535 bytes are not supported.");

        int clustersNeeded = HuBasicWriteRules.GetClustersNeeded(data.Length, _config);

        var allocatedClusters = AllocateClusters(clustersNeeded);
        if (allocatedClusters.Count < clustersNeeded)
            throw new FileSystemException("Not enough free space");

        WriteDataToClusters(data, allocatedClusters);

        // Update FAT
        var fat = _fatManager.ReadFat();
        int terminalFlag = HuBasicWriteRules.GetTerminalFlagForLength(data.Length, _config);
        HuBasicFatRules.ApplyChain(fat, allocatedClusters, terminalFlag);
        _fatManager.WriteFat(fat);

        // Create and add directory entry
        var entry = HuBasicDirectoryRules.CreateFileEntryForWrite(fileName, data, attributes, allocatedClusters[0], loadAddress, executionAddress);
        
        if (forcedRawName != null || forcedRawExtension != null || forcedModifiedAt != null || forcedMetadata != null)
        {
            entry = entry with
            {
                RawFileName = forcedRawName ?? entry.RawFileName,
                RawExtension = forcedRawExtension ?? entry.RawExtension,
                LastModifiedAt = forcedModifiedAt ?? entry.LastModifiedAt,
                FileSystemMetadata = forcedMetadata ?? entry.FileSystemMetadata
            };
        }

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
                    var (name, ext) = HuBasicNameRules.ParseFileName(newName);
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
            isAscii,
            "");
    }

    public void Format()
    {
        if (_diskContainer.IsReadOnly)
            throw new InvalidOperationException("Cannot format read-only disk");

        foreach (var sector in _diskContainer.GetAllSectors())
        {
            var erased = new byte[sector.Size];
            Array.Fill(erased, (byte)0xE5);
            _diskContainer.WriteSector(sector.Cylinder, sector.Head, sector.Sector, erased);
        }

        var fatData = new byte[_config.FatSectors * _config.SectorSize];
        for (int i = 0; i < _config.ReservedClusters; i++)
        {
            int next = i == 0 && _config.ReservedClusters > 1 ? 0x01 : 0x8F;
            _fatManager.SetFatEntry(fatData, i, (ushort)next);
        }

        int activeFatEntries = _config.FatSectors * 128;
        for (int cluster = _config.TotalClusters; cluster < activeFatEntries; cluster++)
        {
            _fatManager.SetFatEntry(fatData, cluster, 0x8F);
        }

        _fatManager.WriteFat(fatData);

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

    public DirectoryEntryLayout ReadDirectoryLayout()
    {
        var items = new List<DirectoryLayoutItem>();
        var order = 0;

        for (int s = 0; s < _config.DirectorySectors; s++)
        {
            var dirData = ReadDirectorySector(s);
            for (int offset = 0; offset < _config.SectorSize; offset += 32)
            {
                byte mode = dirData[offset];
                if (mode == 0xFF)
                {
                    return new DirectoryEntryLayout("Hu-BASIC", items);
                }

                if (mode == 0x00 || mode == 0xE5)
                {
                    continue;
                }

                var entryData = new byte[32];
                Array.Copy(dirData, offset, entryData, 0, 32);
                var entry = _dirParser.Parse(entryData);
                var id = $"{s:D2}:{offset:D3}:{entry.FullName}";
                var itemKind = HuBasicLabelRules.IsVirtualLabelEntry(entry) ? DirectoryLayoutItemKind.VirtualLabel : DirectoryLayoutItemKind.FileEntry;
                var virtualLabel = itemKind == DirectoryLayoutItemKind.VirtualLabel
                    ? new VirtualDirectoryLabelEntry(
                        entry.FileName,
                        entry.Extension,
                        entry.Attributes.RawAttributes,
                        (entry.FileSystemMetadata as HuBasicFileMetadata)?.PasswordByte ?? 0x20,
                        (ushort)entry.Size,
                        entry.LoadAddress ?? 0,
                        entry.EndAddress ?? 0,
                        entry.ExecutionAddress ?? 0,
                        entry.StartCluster
                    )
                    : null;

                var item = new DirectoryLayoutItem(id, order++, itemKind, entry.FullName, entry, virtualLabel);
                if (TryMergeVirtualLabelExtension(items, item))
                {
                    continue;
                }

                items.Add(item);
            }
        }

        return new DirectoryEntryLayout("Hu-BASIC", items);
    }

    public void ApplyDirectoryLayout(DirectoryEntryLayout layout)
    {
        if (_diskContainer.IsReadOnly)
        {
            throw new InvalidOperationException("Disk is read-only");
        }

        var emptySector = new byte[_config.SectorSize];
        Array.Fill(emptySector, (byte)0x00);
        for (var sectorIndex = 0; sectorIndex < _config.DirectorySectors; sectorIndex++)
        {
            WriteDirectorySector(sectorIndex, emptySector.ToArray());
        }

        var orderedItems = layout.Items.OrderBy(item => item.Order).ToArray();
        var entryCapacity = _config.DirectorySectors * (_config.SectorSize / 32);
        if (orderedItems.Length >= entryCapacity)
        {
            throw new FileSystemException("Directory layout exceeds capacity");
        }

        var currentSector = 0;
        var currentOffset = 0;
        var sectorBuffer = new byte[_config.SectorSize];
        Array.Fill(sectorBuffer, (byte)0x00);

        foreach (var item in orderedItems)
        {
            if (currentOffset >= _config.SectorSize)
            {
                WriteDirectorySector(currentSector, sectorBuffer);
                currentSector++;
                currentOffset = 0;
                sectorBuffer = new byte[_config.SectorSize];
                Array.Fill(sectorBuffer, (byte)0x00);
            }

            var entry = item.Kind == DirectoryLayoutItemKind.VirtualLabel
                ? CreateVirtualLabelFileEntry(item.VirtualLabel!)
                : item.Entry!;
            _dirParser.WriteToBuffer(sectorBuffer, currentOffset, entry);
            currentOffset += 32;
        }

        if (currentOffset >= _config.SectorSize)
        {
            WriteDirectorySector(currentSector, sectorBuffer);
            currentSector++;
            currentOffset = 0;
            sectorBuffer = new byte[_config.SectorSize];
            Array.Fill(sectorBuffer, (byte)0x00);
        }

        if (currentSector < _config.DirectorySectors)
        {
            sectorBuffer[currentOffset] = 0xFF;
            WriteDirectorySector(currentSector, sectorBuffer);
            currentSector++;
        }

        for (; currentSector < _config.DirectorySectors; currentSector++)
        {
            var remaining = new byte[_config.SectorSize];
            Array.Fill(remaining, (byte)0x00);
            if (currentSector == _config.DirectorySectors - 1 || currentSector > 0)
            {
                remaining[0] = 0xFF;
            }
            WriteDirectorySector(currentSector, remaining);
        }
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
        return HuBasicAllocationRules.CollectFreeClusters(fat, _diskContainer.DiskType, _config, count);
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

    private static bool IsAscii(ExtendedFileAttributes attributes)
    {
        return attributes.IsAscii || (attributes.RawAttributes & 0x0C) != 0;
    }

    private static bool TryMergeVirtualLabelExtension(List<DirectoryLayoutItem> items, DirectoryLayoutItem item)
    {
        if (item.Kind != DirectoryLayoutItemKind.VirtualLabel || item.VirtualLabel == null || items.Count == 0)
        {
            return false;
        }

        var previous = items[^1];
        if (previous.Kind != DirectoryLayoutItemKind.VirtualLabel || previous.VirtualLabel == null)
        {
            return false;
        }

        if (!HuBasicLabelRules.CanMergeLabelEntries(previous.VirtualLabel, item.VirtualLabel))
        {
            return false;
        }

        var mergedLabel = previous.VirtualLabel with
        {
            Extension = item.VirtualLabel.FileName[1..]
        };
        items[^1] = previous with
        {
            DisplayName = HuBasicNameRules.BuildDisplayName(mergedLabel.FileName, mergedLabel.Extension),
            VirtualLabel = mergedLabel
        };
        return true;
    }

    private static FileEntry CreateVirtualLabelFileEntry(VirtualDirectoryLabelEntry label)
    {
        var metadata = new HuBasicFileMetadata(
            HuBasicFileType.Ascii,
            true,
            false,
            false,
            true,
            false,
            label.Size,
            label.LoadAddress,
            label.ExecutionAddress,
            label.StartCluster,
            label.RawModeByte,
            label.PasswordByte
        );

        return new FileEntry(
            label.FileName,
            label.Extension,
            label.Size,
            null,
            DateTime.Now,
            new ExtendedFileAttributes(DomainAttr.ReadOnly, label.RawModeByte, true, "Hu-BASIC"),
            label.StartCluster,
            label.LoadAddress,
            label.EndAddress,
            label.ExecutionAddress,
            null,
            null,
            metadata
        );
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

    public void Dispose() { }
}
