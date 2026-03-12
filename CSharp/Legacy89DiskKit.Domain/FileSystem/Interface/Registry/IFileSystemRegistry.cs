using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Domain.FileSystem.Interface.Registry;

public interface IFileSystemRegistry
{
    void Register(IFileSystemProvider provider);
    IFileSystem? DetectAndCreate(IDiskContainer container);
}
