using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Factory;

public class FileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IDiskContainer diskContainer)
    {
        // For now, default to Hu-BASIC
        // In the future, we will have a detector to choose MSX-DOS/FAT12 etc.
        return new HuBasicFileSystem(diskContainer);
    }
}
