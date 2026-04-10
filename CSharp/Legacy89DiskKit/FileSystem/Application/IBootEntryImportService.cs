using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public interface IBootEntryImportService
{
    void ImportEntry(IDiskContainer container, IFileSystem fileSystem, BootEntryImportMetadata metadata, byte[] payload);
}
