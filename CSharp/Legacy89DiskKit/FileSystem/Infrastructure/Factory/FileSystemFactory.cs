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
            FileSystemType.Cpm => new CpmFileSystem(container),
            FileSystemType.Cdos => new CdosFileSystem(container),
            FileSystemType.N88Basic => new N88BasicFileSystem(container),
            FileSystemType.MsxDos => new MsxDosFileSystem(container),
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
            
            
            // Check for Hu-BASIC signature first (256-byte sectors)
            if (bootSector.Length >= 32)
            {
                // Hu-BASIC has different structure - check for typical values
                var extension = Encoding.ASCII.GetString(bootSector, 14, 3).Trim();
                if (extension == "SYS" || extension == "Sys")
                {
                    return FileSystemType.HuBasic;
                }
                
                // Additional check: Hu-BASIC usually has bootable flag at offset 0
                var bootFlag = bootSector[0];
                if (bootFlag == 0x01 || bootFlag == 0x00)
                {
                    // Check if it looks like Hu-BASIC structure
                    var diskName = Encoding.ASCII.GetString(bootSector, 1, 13).TrimEnd('\0', ' ');
                    if (!string.IsNullOrWhiteSpace(diskName) || bootFlag == 0x01)
                    {
                        return FileSystemType.HuBasic;
                    }
                }
            }
            
            if (bootSector.Length >= 512)
            {
                // Check for FAT12/16 signature
                var fileSystemType = Encoding.ASCII.GetString(bootSector, 54, 8).Trim();
                if (fileSystemType.Contains("FAT12") || fileSystemType.Contains("FAT"))
                {
                    // Check if it's MSX-DOS specific (MSX has different cluster size)
                    var sectorsPerCluster = bootSector[13];
                    var rootEntries = BitConverter.ToUInt16(bootSector, 17);
                    var mediaDescriptor = bootSector[21];
                    
                    // MSX-DOS typically has 2 sectors/cluster and 112 root entries
                    if (sectorsPerCluster == 2 && rootEntries == 112 && mediaDescriptor == 0xF9)
                    {
                        return FileSystemType.MsxDos;
                    }
                    
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
                        // Check for MSX-DOS specific configuration
                        var rootEntries = BitConverter.ToUInt16(bootSector, 17);
                        var mediaDescriptor = bootSector[21];
                        
                        if (sectorsPerCluster == 2 && rootEntries == 112 && mediaDescriptor == 0xF9)
                        {
                            return FileSystemType.MsxDos;
                        }
                        
                        return FileSystemType.Fat12;
                    }
                }
                
                // Check for N88-BASIC signature (check system track location)
                if (container.DiskType == DiskType.TwoD || container.DiskType == DiskType.TwoDD)
                {
                    try
                    {
                        // Try N88-BASIC system track locations
                        var (systemTrack, systemHead) = container.DiskType == DiskType.TwoD ? (18, 1) : (40, 0);
                        
                        // Try to read directory area (sector 1 on system track)
                        var directoryData = container.ReadSector(systemTrack, systemHead, 1);
                        if (directoryData != null && directoryData.Length > 0)
                        {
                            // Check for N88-BASIC directory structure (16-byte entries)
                            var hasValidEntries = false;
                            for (int offset = 0; offset < Math.Min(directoryData.Length, 256); offset += 16)
                            {
                                if (offset + 16 <= directoryData.Length)
                                {
                                    var firstByte = directoryData[offset];
                                    // Check for valid entry patterns (not all FF or 00)
                                    if (firstByte != 0xFF && firstByte != 0x00 && firstByte >= 0x20 && firstByte <= 0x7E)
                                    {
                                        // Looks like ASCII filename start
                                        hasValidEntries = true;
                                        break;
                                    }
                                }
                            }
                            
                            if (hasValidEntries)
                            {
                                return FileSystemType.N88Basic;
                            }
                        }
                    }
                    catch
                    {
                        // N88-BASIC check failed, continue to other checks
                    }
                }
                
                // Check for CP/M filesystem
                if (CheckCpmSignature(container))
                {
                    return FileSystemType.Cpm;
                }
                
                // Check for CDOS filesystem
                if (CheckCdosSignature(container))
                {
                    return FileSystemType.Cdos;
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
            FileSystemType.Fat12,
            FileSystemType.N88Basic,
            FileSystemType.MsxDos,
            FileSystemType.Cpm,
            FileSystemType.Cdos
        };
    }

    private bool ValidateFileSystemStructure(IDiskContainer container, FileSystemType fileSystemType)
    {
        try
        {
            // For newly created/formatted disks, be more permissive
            if (!container.SectorExists(0, 0, 1))
            {
                // If boot sector doesn't exist, assume it's a new disk that can be formatted
                return true;
            }
            
            var bootSector = container.ReadSector(0, 0, 1);
            
            // Check if it's a blank/unformatted disk
            if (IsBlankSector(bootSector))
            {
                // Blank disk can be formatted to any filesystem
                return true;
            }
            
            return fileSystemType switch
            {
                FileSystemType.Fat12 => ValidateFat12Structure(bootSector),
                FileSystemType.HuBasic => ValidateHuBasicStructure(bootSector),
                FileSystemType.MsxDos => ValidateMsxDosStructure(bootSector),
                FileSystemType.Cpm => ValidateCpmStructure(container),
                FileSystemType.Cdos => ValidateCdosStructure(container),
                FileSystemType.N88Basic => ValidateN88BasicStructure(container),
                _ => false
            };
        }
        catch
        {
            // If any error occurs during validation, assume it's compatible
            return true;
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

    private bool ValidateMsxDosStructure(byte[] bootSector)
    {
        if (bootSector.Length < 512) return false;
        
        // Check boot signature
        if (bootSector[510] != 0x55 || bootSector[511] != 0xAA) return false;
        
        // Check MSX-DOS specific BPB structure
        var bytesPerSector = BitConverter.ToUInt16(bootSector, 11);
        var sectorsPerCluster = bootSector[13];
        var numberOfFats = bootSector[16];
        var rootEntries = BitConverter.ToUInt16(bootSector, 17);
        var mediaDescriptor = bootSector[21];
        
        // MSX-DOS specific values
        return bytesPerSector == 512 && 
               sectorsPerCluster == 2 &&       // MSX固有
               numberOfFats > 0 && 
               rootEntries == 112 &&           // MSX固有
               (mediaDescriptor == 0xF9 || mediaDescriptor == 0xF8 || mediaDescriptor == 0xF0);
    }

    private bool IsBlankSector(byte[] sector)
    {
        // Check if sector is all zeros (unformatted)
        return sector.All(b => b == 0x00);
    }

    private bool CheckCpmSignature(IDiskContainer container)
    {
        try
        {
            // CP/M directory typically starts at track 2
            var directorySector = container.ReadSector(2, 0, 1);
            
            // Check for CP/M directory entry patterns
            for (int offset = 0; offset < directorySector.Length; offset += 32)
            {
                var userNumber = directorySector[offset];
                
                // Valid user numbers are 0-15 or 0xE5 (deleted)
                if (userNumber <= 15 || userNumber == 0xE5)
                {
                    // Check if filename looks valid (ASCII characters)
                    bool isValidFileName = true;
                    for (int i = 1; i <= 11; i++)
                    {
                        var ch = directorySector[offset + i];
                        if (ch != 0x20 && (ch < 0x21 || ch > 0x7E))
                        {
                            isValidFileName = false;
                            break;
                        }
                    }
                    
                    if (isValidFileName)
                        return true;
                }
            }
        }
        catch
        {
            // If we can't read the directory, it's not CP/M
        }
        
        return false;
    }

    private bool CheckCdosSignature(IDiskContainer container)
    {
        try
        {
            // CDOS directory typically starts at track 1
            var directorySector = container.ReadSector(1, 0, 1);
            
            // Check for CDOS directory entry patterns (32-byte entries)
            for (int offset = 0; offset < directorySector.Length; offset += 32)
            {
                if (offset + 32 > directorySector.Length)
                    break;
                    
                var firstByte = directorySector[offset];
                
                // Skip empty or deleted entries
                if (firstByte == 0x00 || firstByte == 0xE5)
                    continue;
                
                // Check if filename looks valid (ASCII characters)
                bool isValidFileName = true;
                for (int i = 0; i < 8; i++)
                {
                    var ch = directorySector[offset + i];
                    if (ch != 0x20 && (ch < 0x21 || ch > 0x7E))
                    {
                        isValidFileName = false;
                        break;
                    }
                }
                
                // Check extension
                if (isValidFileName)
                {
                    for (int i = 8; i < 11; i++)
                    {
                        var ch = directorySector[offset + i];
                        if (ch != 0x20 && (ch < 0x21 || ch > 0x7E))
                        {
                            isValidFileName = false;
                            break;
                        }
                    }
                }
                
                // Check file size (should be reasonable)
                if (isValidFileName)
                {
                    var fileSize = BitConverter.ToUInt32(directorySector, offset + 0x0D);
                    var startTrack = directorySector[offset + 0x11];
                    var startSector = directorySector[offset + 0x12];
                    
                    // Reasonable file size and start position
                    if (fileSize < 0x1000000 && startTrack < 80 && startSector > 0 && startSector <= 26)
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            // If we can't read the directory, it's not CDOS
        }
        
        return false;
    }

    private bool ValidateCpmStructure(IDiskContainer container)
    {
        // CP/M is valid if we can detect the directory structure
        return CheckCpmSignature(container);
    }

    private bool ValidateCdosStructure(IDiskContainer container)
    {
        // CDOS is valid if we can detect the directory structure
        return CheckCdosSignature(container);
    }

    private bool ValidateN88BasicStructure(IDiskContainer container)
    {
        try
        {
            // Try N88-BASIC system track locations
            var (systemTrack, systemHead) = container.DiskType == DiskType.TwoD ? (18, 1) : (40, 0);
            
            // Try to read directory area
            var directoryData = container.ReadSector(systemTrack, systemHead, 1);
            if (directoryData != null && directoryData.Length > 0)
            {
                // Check for valid N88-BASIC directory entries
                for (int offset = 0; offset < Math.Min(directoryData.Length, 256); offset += 16)
                {
                    if (offset + 16 <= directoryData.Length)
                    {
                        var firstByte = directoryData[offset];
                        if (firstByte != 0xFF && firstByte != 0x00 && firstByte >= 0x20 && firstByte <= 0x7E)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch
        {
            // If validation fails, assume it's not N88-BASIC
        }
        
        return false;
    }
}