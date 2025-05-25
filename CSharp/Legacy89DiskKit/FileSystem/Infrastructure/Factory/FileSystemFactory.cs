using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Factory;

public class FileSystemFactory : IFileSystemFactory
{
    public IFileSystem CreateFileSystem(IDiskContainer container, FileSystemType fileSystemType)
    {
        return fileSystemType switch
        {
            FileSystemType.HuBasic => new HuBasicFileSystem(container),
            FileSystemType.Fat12 => new Fat12FileSystem(container),
            FileSystemType.Fat16 => throw new NotImplementedException("FAT16 filesystem not yet implemented"),
            FileSystemType.Cpm => throw new NotImplementedException("CP/M filesystem not yet implemented"),
            FileSystemType.N88Basic => throw new NotImplementedException("N88-BASIC filesystem not yet implemented"),
            FileSystemType.MsxDos => throw new NotImplementedException("MSX-DOS filesystem not yet implemented"),
            _ => throw new ArgumentException($"Unsupported filesystem type: {fileSystemType}")
        };
    }

    public IFileSystem OpenFileSystem(IDiskContainer container, FileSystemType? fileSystemType = null)
    {
        var detectedType = fileSystemType ?? DetectFileSystemType(container);
        return CreateFileSystem(container, detectedType);
    }

    public FileSystemType DetectFileSystemType(IDiskContainer container)
    {
        try
        {
            // Try to read boot sector
            var bootSector = container.ReadSector(0, 0, 1);
            
            if (bootSector.Length >= 512)
            {
                // Check for FAT12/16 signature
                var fileSystemType = Encoding.ASCII.GetString(bootSector, 54, 8).Trim();
                if (fileSystemType.Contains("FAT12") || fileSystemType.Contains("FAT"))
                {
                    return FileSystemType.Fat12;
                }
                
                // Check for boot signature
                if (bootSector.Length >= 512 && bootSector[510] == 0x55 && bootSector[511] == 0xAA)
                {
                    // Has boot signature, check other FAT indicators
                    var bytesPerSector = BitConverter.ToUInt16(bootSector, 11);
                    var sectorsPerCluster = bootSector[13];
                    var numberOfFats = bootSector[16];
                    
                    if (bytesPerSector == 512 && sectorsPerCluster > 0 && numberOfFats > 0)
                    {
                        return FileSystemType.Fat12;
                    }
                }
                
                // Check for Hu-BASIC signature (32-byte boot sector format)
                if (bootSector.Length >= 32)
                {
                    // Hu-BASIC has different structure - check for typical values
                    var extension = Encoding.ASCII.GetString(bootSector, 14, 3).Trim();
                    if (extension == "SYS" || extension == "Sys")
                    {
                        return FileSystemType.HuBasic;
                    }
                }
            }
        }
        catch
        {
            // If reading fails, try Hu-BASIC as fallback
        }

        // Default to Hu-BASIC for backward compatibility
        return FileSystemType.HuBasic;
    }

    public IEnumerable<FileSystemType> GetSupportedFileSystemTypes()
    {
        return new[]
        {
            FileSystemType.HuBasic,
            FileSystemType.Fat12
            // Additional types will be added as they are implemented
        };
    }
}