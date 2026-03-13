using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Interface.Factory;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Infrastructure.DiskImage.Factory;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;

namespace Legacy89DiskKit.Application.DiskImage;

public class DiskService : IDisposable
{
    private readonly IDiskContainerFactory _containerFactory;
    private readonly IFileSystemRegistry _fsRegistry;
    private IDiskContainer? _currentContainer;
    private IFileSystem? _currentFileSystem;

    public DiskService(IDiskContainerFactory? containerFactory = null, IFileSystemRegistry? fsRegistry = null)
    {
        _containerFactory = containerFactory ?? new DiskContainerFactory();
        
        if (fsRegistry == null)
        {
            var defaultRegistry = new FileSystemRegistry();
            defaultRegistry.Register(new HuBasicFileSystemProvider());
            _fsRegistry = defaultRegistry;
        }
        else
        {
            _fsRegistry = fsRegistry;
        }
    }

    public IDiskContainer OpenDisk(string filePath, bool readOnly = true)
    {
        CloseDisk();
        _currentContainer = _containerFactory.Open(filePath, readOnly);
        _currentFileSystem = _fsRegistry.DetectAndCreate(_currentContainer);
        return _currentContainer;
    }

    public IDiskContainer OpenDisk(byte[] imageData, string imageFormat, bool readOnly = true)
    {
        CloseDisk();
        _currentContainer = _containerFactory.Open(imageData, imageFormat, readOnly);
        _currentFileSystem = _fsRegistry.DetectAndCreate(_currentContainer);
        return _currentContainer;
    }

    public IDiskContainer CreateDisk(string filePath, Legacy89DiskKit.Domain.DiskImage.Model.DiskType diskType, string diskName = "")
    {
        CloseDisk();
        _currentContainer = _containerFactory.Create(filePath, diskType, diskName);
        _currentFileSystem = _fsRegistry.DetectAndCreate(_currentContainer);
        return _currentContainer;
    }

    public IFileSystem? FileSystem => _currentFileSystem;

    public void CloseDisk()
    {
        _currentFileSystem?.Dispose();
        _currentFileSystem = null;
        _currentContainer?.Dispose();
        _currentContainer = null;
    }

    public void Dispose()
    {
        CloseDisk();
    }
}
