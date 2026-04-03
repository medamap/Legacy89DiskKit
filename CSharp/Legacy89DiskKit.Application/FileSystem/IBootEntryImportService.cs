using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Application.FileSystem;

public interface IBootEntryImportService
{
    void ImportEntry(IDiskContainer container, IFileSystem fileSystem, BootEntryImportMetadata metadata, byte[] payload);
}
