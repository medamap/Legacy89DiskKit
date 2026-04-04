using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.Native.Domain;

public interface INativeDiskSession : IDiskContainer, IDisposable
{
    IFileSystem? FileSystem { get; }

    DiskContainerMetadata? GetContainerMetadata();

    void CloseDisk();
}
