using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Infrastructure.FileSystem.Msx.Provider;

public class MsxDosFileSystemProvider : IFileSystemProvider
{
    public string FileSystemName => "MSX-DOS";
    public bool CanHandle(IDiskContainer container)
    {
        try
        {
            // MSX-DOS detection:
            // 1. Check if LBA 0 (Boot Sector) starts with a jump (0xEB or 0xE9)
            // 2. Read Sector 1 (likely start of FAT) and check for Media Descriptor
            var bootData = container.ReadSector(0, 0, 1);
            if (bootData.Length > 0 && (bootData[0] == 0xEB || bootData[0] == 0xE9))
            {
                // Most standard MSX-DOS images
                return true;
            }

            // Fallback: Check FAT Media Descriptor at Sector 1, Cylinder 0, Head 0?
            // (Assuming standard 9 sectors/track for 2DD MSX disks)
            var fatSector = container.ReadSector(0, 0, 2); 
            if (fatSector.Length > 0)
            {
                byte media = fatSector[0];
                // F8: 360KB (1-sided), F9: 720KB (2-sided), FA: 320KB, FB: 640KB
                if (media >= 0xF8) return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    public IFileSystem Create(IDiskContainer container)
    {
        return new MsxDosFileSystem(container);
    }
}
