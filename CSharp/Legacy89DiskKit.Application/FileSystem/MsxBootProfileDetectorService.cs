using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Application.FileSystem;

public class MsxBootProfileDetectorService
{
    public string? DetectProfile(IFileSystem fileSystem)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();
        if (fsInfo.FileSystemName == "MSX-DOS")
        {
            var bootArea = fileSystem.ReadBootArea();
            if (bootArea != null && bootArea.Any(b => b != 0x00))
            {
                return "MSX_DOS";
            }
        }

        return null;
    }
}
