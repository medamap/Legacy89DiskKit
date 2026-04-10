using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Exception;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;

namespace Legacy89DiskKit.DiskImage.Infrastructure.Factory;

public class DiskContainerFactory : IDiskContainerFactory
{
    public IDiskContainer OpenDiskImage(string filePath, bool readOnly = false)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Disk image file not found: {filePath}");
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fileInfo = new FileInfo(filePath);
        
        return extension switch
        {
            ".d88" => new D88DiskContainer(filePath, readOnly),
            ".dsk" => new DskDiskContainer(filePath, readOnly),
            ".2d" => ValidateAndCreate2DDiskContainer(filePath, fileInfo.Length, readOnly),
            _ => throw new NotSupportedException($"Unsupported disk image format: {extension}")
        };
    }

    private static IDiskContainer ValidateAndCreate2DDiskContainer(string filePath, long fileSize, bool readOnly)
    {
        const long Expected2DSize = 327680; // 320KB
        
        if (fileSize == Expected2DSize)
        {
            return new TwoDDiskContainer(filePath, readOnly);
        }
        else
        {
            throw new DiskImageException(
                $"Invalid 2D file size: {fileSize} bytes. " +
                "2D format must be exactly 327,680 bytes (320KB).");
        }
    }

    public IDiskContainer CreateNewDiskImage(string filePath, DiskType diskType, string diskName)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".d88" => D88DiskContainer.CreateNew(filePath, diskType, diskName),
            ".dsk" => DskDiskContainer.CreateNew(filePath, diskType, diskName),
            ".2d" => CreateNew2DDiskImage(filePath, diskType, diskName),
            _ => throw new NotSupportedException($"Cannot create disk image format: {extension}")
        };
    }

    private static IDiskContainer CreateNew2DDiskImage(string filePath, DiskType diskType, string diskName)
    {
        if (diskType != DiskType.TwoD)
        {
            throw new ArgumentException(
                "2D format only supports 2D disk type (320KB)");
        }

        const int TotalSize = 327680; // 320KB
        byte[] emptyDisk = new byte[TotalSize];

        for (int i = 0; i < emptyDisk.Length; i++)
        {
            emptyDisk[i] = 0xE5;
        }

        File.WriteAllBytes(filePath, emptyDisk);

        return new TwoDDiskContainer(filePath, false);
    }
}