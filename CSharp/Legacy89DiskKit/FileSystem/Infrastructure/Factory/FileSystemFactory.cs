using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;
using Legacy89DiskKit.FileSystem.Infrastructure.FileSystem;
using Legacy89DiskKit.FileSystem.Domain.Exception;
using System.Text;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Factory;

public class FileSystemFactory : IFileSystemFactory
{
    public IFileSystem CreateFileSystem(IDiskContainer container, FileSystemType fileSystemType)
    {
        IFileSystem fileSystem = fileSystemType switch
        {
            FileSystemType.HuBasic => new HuBasicFileSystem(container),
            FileSystemType.Fat12 => new Fat12FileSystem(container),
            FileSystemType.Fat16 => throw new NotImplementedException("FAT16 filesystem not yet implemented"),
            FileSystemType.Cpm => throw new NotImplementedException("CP/M filesystem not yet implemented"),
            FileSystemType.N88Basic => throw new NotImplementedException("N88-BASIC filesystem not yet implemented"),
            FileSystemType.MsxDos => throw new NotImplementedException("MSX-DOS filesystem not yet implemented"),
            _ => throw new ArgumentException($"Unsupported filesystem type: {fileSystemType}")
        };

        // 新規作成時は構造検証をスキップ（まだフォーマットされていないため）
        return fileSystem;
    }

    public IFileSystem OpenFileSystemReadOnly(IDiskContainer container)
    {
        var detectedType = GuessFileSystemType(container);
        var fileSystem = CreateFileSystem(container, detectedType);
        return new ReadOnlyFileSystemWrapper(fileSystem);
    }

    public IFileSystem OpenFileSystem(IDiskContainer container, FileSystemType fileSystemType)
    {
        var fileSystem = CreateFileSystem(container, fileSystemType);
        
        // 指定されたファイルシステムで実際に読み取り可能かチェック
        if (!ValidateFileSystemStructure(container, fileSystemType))
        {
            throw new FileSystemException(
                $"Disk is not a valid {fileSystemType} filesystem. " +
                $"Use 'info' command to detect actual filesystem type.");
        }
        
        return fileSystem;
    }

    public FileSystemType GuessFileSystemType(IDiskContainer container)
    {
        try
        {
            var bootSector = container.ReadSector(0, 0, 1);
            
            // Check for unformatted disk (all zeros)
            if (IsBlankSector(bootSector))
            {
                throw new FileSystemException("Disk is not formatted. Use 'format' command first.");
            }
            
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
            
            // If we get here, filesystem is unknown
            throw new FileSystemException("Unknown or corrupted filesystem format.");
        }
        catch (FileSystemException)
        {
            throw; // Re-throw filesystem exceptions
        }
        catch (Exception ex)
        {
            throw new FileSystemException($"Failed to detect filesystem: {ex.Message}", ex);
        }
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

    private bool ValidateFileSystemStructure(IDiskContainer container, FileSystemType fileSystemType)
    {
        try
        {
            var bootSector = container.ReadSector(0, 0, 1);
            
            return fileSystemType switch
            {
                FileSystemType.Fat12 => ValidateFat12Structure(bootSector),
                FileSystemType.HuBasic => ValidateHuBasicStructure(bootSector),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateFat12Structure(byte[] bootSector)
    {
        if (bootSector.Length < 512) return false;
        
        // Check boot signature
        if (bootSector[510] != 0x55 || bootSector[511] != 0xAA) return false;
        
        // Check basic FAT12 structure
        var bytesPerSector = BitConverter.ToUInt16(bootSector, 11);
        var sectorsPerCluster = bootSector[13];
        var numberOfFats = bootSector[16];
        var rootEntries = BitConverter.ToUInt16(bootSector, 17);
        
        return bytesPerSector == 512 && 
               sectorsPerCluster > 0 && 
               numberOfFats > 0 && 
               rootEntries > 0;
    }

    private bool ValidateHuBasicStructure(byte[] bootSector)
    {
        if (bootSector.Length < 32) return false;
        
        // Check for Hu-BASIC boot sector structure
        // Boot flag should be 0x00 or 0x01
        var bootFlag = bootSector[0];
        if (bootFlag != 0x00 && bootFlag != 0x01) return false;
        
        // Check for typical file extension
        var extension = Encoding.ASCII.GetString(bootSector, 14, 3).Trim().ToUpper();
        return extension == "SYS" || extension == "";
    }

    private bool IsBlankSector(byte[] sector)
    {
        // Check if sector is all zeros (unformatted)
        return sector.All(b => b == 0x00);
    }
}