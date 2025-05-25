using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.Factory;

public interface IFileSystemFactory
{
    IFileSystem CreateFileSystem(IDiskContainer container, FileSystemType fileSystemType);
    IFileSystem OpenFileSystem(IDiskContainer container, FileSystemType? fileSystemType = null);
    FileSystemType DetectFileSystemType(IDiskContainer container);
    IEnumerable<FileSystemType> GetSupportedFileSystemTypes();
}

public enum FileSystemType
{
    HuBasic,
    Fat12,
    Fat16,
    Cpm,
    N88Basic,
    MsxDos
}