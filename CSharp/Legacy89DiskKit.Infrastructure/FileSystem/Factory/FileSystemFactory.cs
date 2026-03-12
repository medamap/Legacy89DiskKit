using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.Factory;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

namespace Legacy89DiskKit.Infrastructure.FileSystem.Factory;

public class FileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IDiskContainer diskContainer)
    {
        // For now, default to Hu-BASIC
        // In the future, we will have a detector to choose MSX-DOS/FAT12 etc.
        return new HuBasicFileSystem(diskContainer);
    }
}
