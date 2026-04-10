using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public interface IBootProfileService
{
    BootInfoSummary GetBootProfile(IFileSystem fileSystem);
}
