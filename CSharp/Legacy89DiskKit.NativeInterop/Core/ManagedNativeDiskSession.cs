using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.NativeInterop.Core;

public sealed class ManagedNativeDiskSession : INativeDiskSession
{
    private readonly DiskService _service;

    public ManagedNativeDiskSession(DiskService service)
    {
        _service = service;
    }

    public IFileSystem? FileSystem => _service.FileSystem;

    public DiskContainerMetadata? GetContainerMetadata()
    {
        return _service.GetContainerMetadata();
    }

    public void CloseDisk()
    {
        _service.CloseDisk();
    }

    public void Dispose()
    {
        _service.Dispose();
    }
}
