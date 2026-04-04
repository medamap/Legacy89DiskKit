using Legacy89DiskKit.Domain.FileSystem.Interface.Registry;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;

namespace Legacy89DiskKit.Infrastructure.FileSystem.Cpm.Provider;

public class CpmFileSystemProvider : IFileSystemProvider
{
    public string FileSystemName => "CP/M";

    public bool CanHandle(IDiskContainer container)
    {
        try
        {
            // Simple heuristic from the old codebase
            // CP/M directory typically starts at track 2
            // We check for valid user numbers (0-15) or deleted marker (0xE5)
            // and check if the filename part (bytes 1-11) is valid ASCII.

            var directorySector = container.ReadSector(2, 0, 1);
            if (directorySector == null || directorySector.Length < 32) return false;

            for (int offset = 0; offset < directorySector.Length; offset += 32)
            {
                if (offset + 32 > directorySector.Length) break;
                
                var userNumber = directorySector[offset];
                
                // Valid user numbers are 0-15 or 0xE5 (deleted)
                if (userNumber <= 15 || userNumber == 0xE5)
                {
                    // Check if filename looks valid (ASCII characters)
                    bool isValidFileName = true;
                    for (int i = 1; i <= 11; i++)
                    {
                        var ch = directorySector[offset + i];
                        // 0x20 is space, 0x21-0x7E are printable ASCII
                        if (ch != 0x20 && (ch < 0x21 || ch > 0x7E))
                        {
                            isValidFileName = false;
                            break;
                        }
                    }
                    
                    if (isValidFileName && userNumber != 0xE5)
                        return true;
                }
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
        return new CpmFileSystem(container);
    }
}
