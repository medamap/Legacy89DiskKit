using Legacy89DiskKit.FileSystem.Domain.Interface.Registry;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Provider;

public class HuBasicFileSystemProvider : IFileSystemProvider
{
    public string FileSystemName => "Hu-BASIC";

    public bool CanHandle(IDiskContainer container)
    {
        try
        {
            var bootData = container.ReadSector(0, 0, 1);
            if (bootData.Length < 32) return false;

            // Hu-BASIC Boot Sector structure:
            // 0x00: Boot Flag (0x01)
            // 0x01-0x0D: Label (e.g. "BASIC CZ8FB01")
            // 0x0E-0x10: Extension ("Sys")
            
            bool hasBootFlag = bootData[0] == 0x01;
            string extension = Encoding.ASCII.GetString(bootData, 0x0E, 3);
            bool isSys = string.Equals(extension, "Sys", StringComparison.OrdinalIgnoreCase);

            if (hasBootFlag && isSys) return true;

            // Fallback: check for "Hu-BASIC" or "BASIC" in the first 32 bytes
            string label = Encoding.ASCII.GetString(bootData, 0, 32);
            return label.Contains("BASIC", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public IFileSystem Create(IDiskContainer container)
    {
        return new HuBasicFileSystem(container);
    }
}
