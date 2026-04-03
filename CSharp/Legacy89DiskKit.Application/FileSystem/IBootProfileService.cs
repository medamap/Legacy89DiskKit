using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Application.FileSystem;

public interface IBootProfileService
{
    BootInfoSummary GetBootProfile(IFileSystem fileSystem);
}
