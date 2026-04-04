using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.Application.FileSystem;
public interface IBootEntryExportService
{
    IReadOnlyList<BootEntryExportArtifact> ExportEntries(IDiskContainer container, IFileSystem fileSystem);
}