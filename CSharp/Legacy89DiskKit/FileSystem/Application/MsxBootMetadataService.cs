using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Application;

namespace Legacy89DiskKit.FileSystem.Application;
public class MsxBootMetadataService
{
    public BootInfoSummary GetBootSummary(IFileSystem fileSystem)
    {
        var fsInfo = fileSystem.GetFileSystemInfo();
        if (fsInfo.FileSystemName == "MSX-DOS")
        {
            var bootArea = fileSystem.ReadBootArea();
            if (bootArea != null && bootArea.Any(b => b != 0x00))
            {
                return new BootInfoSummary(BootInfoMode.SectorResident, MachineFamily: MachineType.MSX);
            }
        }

        return new BootInfoSummary(BootInfoMode.None);
    }
}
