using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.Registry;

public interface IFileSystemRegistry
{
    void Register(IFileSystemProvider provider);
    IFileSystem? DetectAndCreate(IDiskContainer container);
}
