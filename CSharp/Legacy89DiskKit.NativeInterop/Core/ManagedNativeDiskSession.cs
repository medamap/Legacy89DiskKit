using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.Native;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class ManagedNativeDiskSession : INativeDiskSession, IDiskContainer
{
    private readonly IDiskContainer _container;
    private readonly IFileSystem _fileSystem;

    public ManagedNativeDiskSession(IDiskContainer container, IFileSystem fileSystem)
    {
        _container = container;
        _fileSystem = fileSystem;
    }

    // INativeDiskSession
    public IFileSystem? FileSystem => _fileSystem;

    public DiskContainerMetadata? GetContainerMetadata()
    {
        return _container.GetMetadata();
    }

    public void CloseDisk()
    {
        Dispose();
    }

    // IDiskContainer
    public string FilePath => _container.FilePath;
    public bool IsReadOnly => _container.IsReadOnly;
    public DiskType DiskType => _container.DiskType;
    public DiskContainerMetadata GetMetadata() => _container.GetMetadata();
    public byte[] ReadSector(int cylinder, int head, int sector) => _container.ReadSector(cylinder, head, sector);
    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted) => _container.ReadSector(cylinder, head, sector, allowCorrupted);
    public void WriteSector(int cylinder, int head, int sector, byte[] data) => _container.WriteSector(cylinder, head, sector, data);
    public bool SectorExists(int cylinder, int head, int sector) => _container.SectorExists(cylinder, head, sector);
    public IEnumerable<SectorInfo> GetAllSectors() => _container.GetAllSectors();
    public void Save() => _container.Save();
    public void SaveAs(string filePath) => _container.SaveAs(filePath);

    public void Dispose()
    {
        _fileSystem.Dispose();
        _container.Dispose();
    }
}
