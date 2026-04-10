using Legacy89DiskKit.Domain.FileSystem.Model;

namespace Legacy89DiskKit.Domain.FileSystem.Interface.Layout;

public interface IDirectoryLayoutProvider
{
    DirectoryEntryLayout ReadDirectoryLayout();
    void ApplyDirectoryLayout(DirectoryEntryLayout layout);
}
