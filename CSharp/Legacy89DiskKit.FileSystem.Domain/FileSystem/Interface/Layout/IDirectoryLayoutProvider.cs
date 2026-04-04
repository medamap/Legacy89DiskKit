using Legacy89DiskKit.FileSystem.Domain.Model;

namespace Legacy89DiskKit.FileSystem.Domain.Interface.Layout;

public interface IDirectoryLayoutProvider
{
    DirectoryEntryLayout ReadDirectoryLayout();
    void ApplyDirectoryLayout(DirectoryEntryLayout layout);
}
