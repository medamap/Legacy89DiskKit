using System.Text;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;
using FileAttributes = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos;

public class XDosFileSystem : IFileSystem
{
    private readonly IDiskContainer    _container;
    private readonly XDosVolumeRecord  _volumeRecord;
    private readonly XDosFatReader     _fat;
    private readonly XDosFamReader     _fam;
    private readonly XDosDirParser     _dirParser;
    private readonly XDosClusterReader _clusterReader;
    private IReadOnlyList<XDosDirectoryEntry>? _cachedDirectory;

    public XDosFileSystem(IDiskContainer container)
    {
        _container     = container;
        _volumeRecord  = ReadVolumeRecord(container);
        _fat           = new XDosFatReader(container);
        _fam           = new XDosFamReader(container);
        _dirParser     = new XDosDirParser();
        _clusterReader = new XDosClusterReader(container, _fam);
    }

    public FileSystemCapabilities Capabilities =>
        FileSystemCapabilities.SupportsBootArea |
        FileSystemCapabilities.SupportsAttributes |
        FileSystemCapabilities.FixedFileNameLength;

    public DiskFileSystemInfo GetFileSystemInfo()
    {
        int freeCount = _fat.CountFreeClusters();
        int usedCount = _fat.CountUsedClusters();
        const int ClusterSize = SectorsPerTrack * SectorSize;
        return new DiskFileSystemInfo(
            "X-DOS",
            TotalCapacity: (long)(freeCount + usedCount) * ClusterSize,
            FreeSpace: (long)freeCount * ClusterSize,
            ClusterSize: ClusterSize,
            ReservedSectors: 0,
            PlatformId: "X1",
            DefaultEncodingId: "X1"
        );
    }

    public IEnumerable<FileEntry> GetFiles()
    {
        var dir = GetDirectory();
        return dir.Select(ToFileEntry);
    }

    public bool FileExists(string fileName)
        => GetDirectory().Any(e => string.Equals(e.FileName, fileName, StringComparison.Ordinal));

    public byte[] ReadFile(string fileName)
    {
        var entry = GetDirectory().FirstOrDefault(
            e => string.Equals(e.FileName, fileName, StringComparison.Ordinal))
            ?? throw new FileNotFoundException($"File not found: {fileName}");
        return _clusterReader.ReadFile(entry);
    }

    public void WriteFile(string fileName, byte[] data, ExtendedFileAttributes attributes,
                          ushort? loadAddress = null, ushort? executionAddress = null)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void DeleteFile(string fileName)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void RenameFile(string oldName, string newName)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void CopyFile(string sourceName, string targetName)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void UpdateAttributes(string fileName, ExtendedFileAttributes attributes)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public ExtendedFileAttributes CreateDefaultAttributes(bool isAscii)
        => new ExtendedFileAttributes(FileAttributes.None, 0x00, isAscii, "X-DOS");

    public void Format()
        => throw new NotSupportedException("X-DOS format support is not yet implemented.");

    public byte[] ReadBootArea()
    {
        var result = new List<byte>();
        for (int r = 1; r <= 16; r++)
            result.AddRange(_container.ReadSector(0, 0, r));
        return result.ToArray();
    }

    public void WriteBootArea(byte[] data)
        => throw new NotSupportedException("X-DOS write support is not yet implemented.");

    public void Dispose() { }

    private IReadOnlyList<XDosDirectoryEntry> GetDirectory()
        => _cachedDirectory ??= _dirParser.Parse(_container);

    private static XDosVolumeRecord ReadVolumeRecord(IDiskContainer container)
    {
        var sector = container.ReadSector(0, 0, 1);
        string label = Encoding.ASCII.GetString(sector, 1, 16).TrimEnd(' ');
        return new XDosVolumeRecord(label, sector[24], sector[25], sector[26], sector[27]);
    }

    private static FileEntry ToFileEntry(XDosDirectoryEntry e) =>
        new FileEntry(
            FileName:           e.FileName,
            Extension:          string.Empty,
            Size:               e.FileSize,
            CreatedAt:          null,
            LastModifiedAt:     null,
            Attributes:         new ExtendedFileAttributes(FileAttributes.None, e.Attribute, false, "X-DOS"),
            LoadAddress:        e.LoadAddress,
            EndAddress:         e.EndAddress,
            ExecutionAddress:   e.ExecutionAddress,
            RawFileName:        e.RawFileName
        );

    private const int SectorsPerTrack = 10;
    private const int SectorSize      = 512;
}
