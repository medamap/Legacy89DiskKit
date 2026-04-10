using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Domain.Native;

public interface INativeDiskSession : IDiskContainer, IDisposable
{
    IFileSystem? FileSystem { get; }

    DiskContainerMetadata? GetContainerMetadata();

    void CloseDisk();
}
