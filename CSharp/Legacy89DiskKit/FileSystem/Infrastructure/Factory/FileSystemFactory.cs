using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Factory;

public class FileSystemFactory : IFileSystemFactory
{
    public IFileSystem CreateFileSystem(IDiskContainer container, FileSystemType fileSystemType)
    {
        return fileSystemType switch
        {
            FileSystemType.HuBasic => new HuBasicFileSystem(container),
            FileSystemType.Fat12 => throw new NotImplementedException("FAT12 filesystem not yet implemented"),
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
            
            // Check for Hu-BASIC signature (simple heuristic)
            // In a real implementation, this would check for specific patterns
            if (bootSector.Length >= 32)
            {
                // For now, default to Hu-BASIC
                // Future implementations would add more sophisticated detection
                return FileSystemType.HuBasic;
            }
        }
        catch
        {
            // If reading fails, default to Hu-BASIC
        }

        return FileSystemType.HuBasic;
    }

    public IEnumerable<FileSystemType> GetSupportedFileSystemTypes()
    {
        return new[]
        {
            FileSystemType.HuBasic
            // Additional types will be added as they are implemented
        };
    }
}