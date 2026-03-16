using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.NativeInterop.Core;

public interface INativeDiskSession : IDisposable
{
    IFileSystem? FileSystem { get; }

    DiskContainerMetadata? GetContainerMetadata();

    void CloseDisk();
}
