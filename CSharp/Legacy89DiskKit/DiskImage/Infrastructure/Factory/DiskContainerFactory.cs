using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
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
        
        return extension switch
        {
            ".d88" => new D88DiskContainer(filePath, readOnly),
            ".dsk" => new DskDiskContainer(filePath, readOnly),
            _ => throw new NotSupportedException($"Unsupported disk image format: {extension}")
        };
    }

    public IDiskContainer CreateNewDiskImage(string filePath, DiskType diskType, string diskName)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".d88" => D88DiskContainer.CreateNew(filePath, diskType, diskName),
            ".dsk" => DskDiskContainer.CreateNew(filePath, diskType, diskName),
            _ => throw new NotSupportedException($"Cannot create disk image format: {extension}")
        };
    }
}