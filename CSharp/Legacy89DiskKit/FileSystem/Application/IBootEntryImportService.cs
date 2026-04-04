using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public interface IBootEntryImportService
{
    void ImportEntry(IDiskContainer container, IFileSystem fileSystem, BootEntryImportMetadata metadata, byte[] payload);
}
