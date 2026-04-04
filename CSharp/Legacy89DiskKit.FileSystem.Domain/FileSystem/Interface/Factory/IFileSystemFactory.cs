using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.Factory;

public interface IFileSystemFactory
{
    IFileSystem Create(IDiskContainer diskContainer);
}
