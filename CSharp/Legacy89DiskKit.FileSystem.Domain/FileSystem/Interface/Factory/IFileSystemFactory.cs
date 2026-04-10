using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Domain.FileSystem.Interface.Factory;

public interface IFileSystemFactory
{
    IFileSystem Create(IDiskContainer diskContainer);
}
