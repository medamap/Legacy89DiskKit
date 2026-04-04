using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.Msx.Models;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using DomainAttr = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Msx;

public class MsxDosFileSystem : IFileSystem
{
    private readonly IDiskContainer _diskContainer;
    private readonly MsxDosFatManager _fatManager;
    private readonly MsxDosDirParser _dirParser;
    
    // BPB Values
    private ushort _bytsPerSec;
    private byte _secPerClus;
    private ushort _rsvdSecCnt;
    private byte _numFATs;
    private ushort _rootEntCnt;
    private ushort _fatSz16;
    private int _dataStartLba;

    public MsxDosFileSystem(IDiskContainer diskContainer)
    {
        _diskContainer = diskContainer;
        _dirParser = new MsxDosDirParser();
        LoadBpb();
        _fatManager = new MsxDosFatManager(diskContainer, _rsvdSecCnt, _fatSz16);
    }

    private void LoadBpb()
    {
        var bootData = _diskContainer.ReadSector(0, 0, 1);
        if (bootData.Length < 64) throw new FileSystemException("Invalid boot sector");

        _bytsPerSec = BitConverter.ToUInt16(bootData, 11);
        _secPerClus = bootData[13];
        _rsvdSecCnt = BitConverter.ToUInt16(bootData, 14);
        _numFATs = bootData[16];
        _rootEntCnt = BitConverter.ToUInt16(bootData, 17);
        _fatSz16 = BitConverter.ToUInt16(bootData, 22);
        
        // Fallback for MSX-DOS 1.0 if BPB is 0 (use Media Descriptor heuristic)
        if (_bytsPerSec == 0)
        {
            // Read Media Descriptor from FAT[0]
            var fatSector = _diskContainer.ReadSector(0, 0, 2); // FAT starts after boot (if RsvdSecCnt=1)
            byte mediaDesc = fatSector[0];
            if (mediaDesc == 0xF9) // 720KB
            {
                _bytsPerSec = 512;
                _secPerClus = 2;
                _rsvdSecCnt = 1;
                _numFATs = 2;
                _rootEntCnt = 112;
                _fatSz16 = 3;
            }
            else // Default to 360KB (0xF8)
            {
                _bytsPerSec = 512;
                _secPerClus = 2;
                _rsvdSecCnt = 1;
                _numFATs = 2;
                _rootEntCnt = 112;
                _fatSz16 = 2;
            }
        }

        int rootDirLba = _rsvdSecCnt + (_numFATs * _fatSz16);
        _dataStartLba = rootDirLba + (_rootEntCnt * 32 / _bytsPerSec);
    }

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        var fat = _fatManager.ReadFat();
        int free = 0;
        int maxCluster = (fat.Length * 2 / 3);
        for (int i = 2; i < maxCluster; i++)
        {
            if (_fatManager.GetFatEntry(fat, i) == 0x000) free++;
        }

        return new DiskFileSystemInfo(
            "MSX-DOS",
            (long)maxCluster * _secPerClus * _bytsPerSec,
            (long)free * _secPerClus * _bytsPerSec,
            _secPerClus * _bytsPerSec,
            _rsvdSecCnt,
            "MSX",
            "SJIS",
            8,
            3
        );
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        var files = new List<FileEntry>();
        int rootDirLba = _rsvdSecCnt + (_numFATs * _fatSz16);
        int rootSectors = (_rootEntCnt * 32) / _bytsPerSec;

        for (int i = 0; i < rootSectors; i++)
        {
            var (c, h, s) = LbaToPhysical(rootDirLba + i);
            var dirData = _diskContainer.ReadSector(c, h, s);
            for (int offset = 0; offset < _bytsPerSec; offset += 32)
            {
                byte marker = dirData[offset];
                if (marker == 0x00) return files; // End
                if (marker == 0xE5) continue; // Deleted

                var entry = _dirParser.Parse(dirData.Skip(offset).Take(32).ToArray());
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
        return data.Length > file.Size ? data.Take((int)file.Size).ToArray() : data;
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes, ushort? loadAddress = null, ushort? executionAddress = null)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        if (FileExists(fileName)) throw new FileSystemException("File already exists");

        int clustersNeeded = (data.Length + (_secPerClus * _bytsPerSec) - 1) / (_secPerClus * _bytsPerSec);
        if (clustersNeeded == 0) clustersNeeded = 1;

        var allocated = AllocateClusters(clustersNeeded);
        if (allocated.Count < clustersNeeded) throw new FileSystemException("No space");

        WriteDataToClusters(data, allocated);

        var fat = _fatManager.ReadFat();
        for (int i = 0; i < allocated.Count; i++)
        {
            ushort next = (i == allocated.Count - 1) ? (ushort)0xFFF : (ushort)allocated[i + 1];
            _fatManager.SetFatEntry(fat, allocated[i], next);
        }
        _fatManager.WriteFat(fat);

        var (name, ext) = ParseFileName(fileName);
        var entry = new FileEntry(name, ext, data.Length, null, DateTime.Now, attributes, allocated[0], null, null, null);
        AddDirectoryEntry(entry);
    }

    public void DeleteFile(string fileName)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        var file = GetFiles().FirstOrDefault(f => f.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (file == null) throw new FileSystemException("Not found");

        var clusters = _fatManager.GetClusterChain(file.StartCluster);
        var fat = _fatManager.ReadFat();
        foreach (var c in clusters) _fatManager.SetFatEntry(fat, c, 0x000);
        _fatManager.WriteFat(fat);

        MarkAsDeleted(fileName);
    }

    public void RenameFile(string oldName, string newName)
    {
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
        UpdateDirEntry(fileName, entry => entry with { Attributes = attributes });
    }

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii)
    {
        return new ExtendedFileAttributes(DomainAttr.None, 0x00, isAscii, "MSX");
    }

    public void Format()
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");

        // 1. Initialize FAT
        // MSX-DOS FAT starts with Media Descriptor followed by two 0xFF bytes for 12-bit FAT end-of-chain
        var fatData = new byte[_fatSz16 * _bytsPerSec];
        
        // Read media descriptor from boot sector (offset 21)
        var bootData = _diskContainer.ReadSector(0, 0, 1);
        byte mediaDesc = bootData[21] != 0 ? bootData[21] : (byte)0xF9; // 0xF9 for 720KB default
        
        fatData[0] = mediaDesc;
        fatData[1] = 0xFF;
        fatData[2] = 0xFF;
        
        for (int i = 0; i < _numFATs; i++)
        {
            _fatManager.WriteFat(fatData); // Note: WriteFat needs to take fat index? 
            // Current WriteFat writes primary. Let's write to both if possible.
            // Actually, MSX-DOS often only uses the first FAT but standard is 2.
            for (int s = 0; s < _fatSz16; s++) {
                var (c, h, sec) = LbaToPhysical(_rsvdSecCnt + i * _fatSz16 + s);
                var sectorBuffer = new byte[_bytsPerSec];
                Array.Copy(fatData, s * _bytsPerSec, sectorBuffer, 0, _bytsPerSec);
                _diskContainer.WriteSector(c, h, sec, sectorBuffer);
            }
        }

        // 2. Initialize Directory
        var emptySector = new byte[_bytsPerSec]; // 0x00 means free
        int rootDirLba = _rsvdSecCnt + (_numFATs * _fatSz16);
        int rootSectors = (_rootEntCnt * 32) / _bytsPerSec;
        for (int i = 0; i < rootSectors; i++)
        {
            var (c, h, s) = LbaToPhysical(rootDirLba + i);
            _diskContainer.WriteSector(c, h, s, emptySector);
        }
    }

    public byte[] ReadBootArea()
    {
        // MSX-DOS usually has a 256-byte boot program, but the sector is 512 bytes.
        return _diskContainer.ReadSector(0, 0, 1);
    }

    public void WriteBootArea(byte[] data)
    {
        if (_diskContainer.IsReadOnly) throw new InvalidOperationException("Disk is read-only");
        if (data.Length > 512) throw new ArgumentException("MSX boot area is limited to 512 bytes (one sector).");
        
        var sectorData = _diskContainer.ReadSector(0, 0, 1);
        Array.Copy(data, sectorData, Math.Min(data.Length, 512));
        _diskContainer.WriteSector(0, 0, 1, sectorData);
    }

    public FileSystemCapabilities Capabilities => 
        FileSystemCapabilities.SupportsAttributes | 
        FileSystemCapabilities.SupportsRename | 
        FileSystemCapabilities.SupportsBootArea;

    private byte[] ReadCluster(int cluster)
    {
        using var ms = new MemoryStream();
        int lba = _dataStartLba + (cluster - 2) * _secPerClus;
        for (int i = 0; i < _secPerClus; i++)
        {
            var (c, h, s) = LbaToPhysical(lba + i);
            ms.Write(_diskContainer.ReadSector(c, h, s));
        }
        return ms.ToArray();
    }

    private void WriteCluster(int cluster, byte[] data)
    {
        int lba = _dataStartLba + (cluster - 2) * _secPerClus;
        for (int i = 0; i < _secPerClus; i++)
        {
            var (c, h, s) = LbaToPhysical(lba + i);
            var sectorData = new byte[_bytsPerSec];
            Array.Copy(data, i * _bytsPerSec, sectorData, 0, _bytsPerSec);
            _diskContainer.WriteSector(c, h, s, sectorData);
        }
    }

    private List<int> AllocateClusters(int count)
    {
        var fat = _fatManager.ReadFat();
        var allocated = new List<int>();
        int maxCluster = fat.Length * 2 / 3;
        for (int i = 2; i < maxCluster && allocated.Count < count; i++)
        {
            if (_fatManager.GetFatEntry(fat, i) == 0x000) allocated.Add(i);
        }
        return allocated;
    }

    private void WriteDataToClusters(byte[] data, List<int> clusters)
    {
        int offset = 0;
        int clusterSize = _secPerClus * _bytsPerSec;
        foreach (var c in clusters)
        {
            var clusterData = new byte[clusterSize];
            int toCopy = Math.Min(data.Length - offset, clusterSize);
            Array.Copy(data, offset, clusterData, 0, toCopy);
            WriteCluster(c, clusterData);
            offset += toCopy;
        }
    }

    private void AddDirectoryEntry(FileEntry entry)
    {
        int rootDirLba = _rsvdSecCnt + (_numFATs * _fatSz16);
        int rootSectors = (_rootEntCnt * 32) / _bytsPerSec;

        for (int i = 0; i < rootSectors; i++)
        {
            var (c, h, s) = LbaToPhysical(rootDirLba + i);
            var dirData = _diskContainer.ReadSector(c, h, s);
            for (int offset = 0; offset < _bytsPerSec; offset += 32)
            {
                byte marker = dirData[offset];
                if (marker == 0x00 || marker == 0xE5)
                {
                    _dirParser.WriteToBuffer(dirData, offset, entry);
                    _diskContainer.WriteSector(c, h, s, dirData);
                    return;
                }
            }
        }
        throw new FileSystemException("Root directory full");
    }

    private void UpdateDirEntry(string fileName, Func<FileEntry, FileEntry?> updateFunc)
    {
        int rootDirLba = _rsvdSecCnt + (_numFATs * _fatSz16);
        int rootSectors = (_rootEntCnt * 32) / _bytsPerSec;

        for (int i = 0; i < rootSectors; i++)
        {
            var (c, h, s) = LbaToPhysical(rootDirLba + i);
            var dirData = _diskContainer.ReadSector(c, h, s);
            bool modified = false;
            for (int offset = 0; offset < _bytsPerSec; offset += 32)
            {
                byte marker = dirData[offset];
                if (marker == 0x00) break;
                if (marker == 0xE5) continue;

                var entry = _dirParser.Parse(dirData.Skip(offset).Take(32).ToArray());
                if (entry.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    var updated = updateFunc(entry);
                    if (updated == null) dirData[offset] = 0xE5;
                    else _dirParser.WriteToBuffer(dirData, offset, updated);
                    modified = true;
                    break;
                }
            }
            if (modified)
            {
                _diskContainer.WriteSector(c, h, s, dirData);
                return;
            }
        }
        throw new FileSystemException("File not found");
    }

    private void MarkAsDeleted(string fileName) { UpdateDirEntry(fileName, entry => null); }

    private (string name, string ext) ParseFileName(string fileName)
    {
        var parts = fileName.Split('.');
        string name = parts[0].ToUpper();
        if (name.Length > 8) name = name.Substring(0, 8);
        string ext = parts.Length > 1 ? parts[parts.Length - 1].ToUpper() : "";
        if (ext.Length > 3) ext = ext.Substring(0, 3);
        return (name, ext);
    }

    private (int c, int h, int s) LbaToPhysical(int lba)
    {
        int trackSize = 9;
        int cylinder = lba / (trackSize * 2);
        int head = (lba / trackSize) % 2;
        int sector = (lba % trackSize) + 1;
        return (cylinder, head, sector);
    }

    public void Dispose() { }
}
