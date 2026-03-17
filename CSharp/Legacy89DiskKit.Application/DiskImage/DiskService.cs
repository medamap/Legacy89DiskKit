using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Interface.Factory;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Infrastructure.DiskImage.Factory;
using Legacy89DiskKit.Application.FileSystem;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Provider;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.Native;

namespace Legacy89DiskKit.Application.DiskImage;

public class DiskService : IDisposable
{
    private readonly INativeBridgeBackend _backend;
    private readonly IFileSystemRegistry? _fsRegistry;
    
    private INativeDiskSession? _currentSession;

    public DiskService(INativeBridgeBackend? backend = null, IFileSystemRegistry? fsRegistry = null)
    {
        _backend = backend ?? NativeBridgeBackend.Current;
        _fsRegistry = fsRegistry;
    }

    public IDiskContainer OpenDisk(string filePath, bool readOnly = true)
    {
        CloseDisk();
        _currentSession = _backend.OpenDisk(filePath, readOnly);
        return _currentSession as IDiskContainer ?? throw new InvalidOperationException("Backend session does not support container access.");
    }

    public IDiskContainer OpenDisk(byte[] imageData, string imageFormat, bool readOnly = true)
    {
        CloseDisk();
        _currentSession = _backend.OpenDisk(imageData, imageFormat, readOnly);
        return _currentSession as IDiskContainer ?? throw new InvalidOperationException("Backend session does not support container access.");
    }

    public IDiskContainer CreateDisk(string filePath, DiskType diskType, string diskName = "")
    {
        CloseDisk();
        _currentSession = _backend.CreateDisk(filePath, diskType, diskName);
        return _currentSession as IDiskContainer ?? throw new InvalidOperationException("Backend session does not support container access.");
    }

    public IFileSystem? FileSystem => _currentSession?.FileSystem;

    public INativeDiskSession? Session => _currentSession;

    public DiskContainerMetadata? GetContainerMetadata()
    {
        return _currentSession?.GetContainerMetadata();
    }

    public void CloseDisk()
    {
        _currentSession?.Dispose();
        _currentSession = null;
    }

    public void Dispose()
    {
        CloseDisk();
    }
}
