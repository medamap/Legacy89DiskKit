using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Infrastructure.FileSystem.Pc88.Provider;

public class N88BasicFileSystemProvider : IFileSystemProvider
{
    public string FileSystemName => "N88-BASIC";
    public bool CanHandle(IDiskContainer container)
    {
        // First check: Media flag in D88 header
        // PC-8801 usually uses 0x00 (2D), 0x10 (2DD), or 0x20 (2HD)
        if (container.DiskType == DiskType.HardDisk) return false;

        try
        {
            // Second check: Validate ID Sector / System Track
            // Standard N88-BASIC (2D) has system info at Track 18, Head 1.
            // ID Sector is Sector 13 on that track.
            
            // Try 2D position
            var idData = container.ReadSector(9, 1, 13); // T18 linear is Cylinder 9, Head 1
            if (idData.Length > 0)
            {
                // Simple heuristic: Byte 0 is media attribute
                // Common values: 0xFE (2D), 0xFB (2DD), etc. (varies)
                // For now, let's also check if the directory sectors (1-12) have some valid entries or are all FF/00.
                var dirData = container.ReadSector(9, 1, 1);
                if (dirData.Length >= 16)
                {
                    byte mode = dirData[0];
                    if (mode == 0x01 || mode == 0x00 || mode == 0xFF) return true;
                }
            }
            
            // Try 2DD position (Cylinder 20, Head 0 for Track 40 linear?)
            // Actually, D88 linear track 40 is Cylinder 20, Head 0.
            var idData2DD = container.ReadSector(20, 0, 13);
            if (idData2DD.Length > 0) return true;
        }
        catch
        {
            return false;
        }

        return false;
    }

    public IFileSystem Create(IDiskContainer container)
    {
        return new N88BasicFileSystem(container);
    }
}
