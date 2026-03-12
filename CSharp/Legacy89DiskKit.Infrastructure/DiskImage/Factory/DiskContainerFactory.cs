using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Interface.Factory;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Factory;

public class DiskContainerFactory : IDiskContainerFactory
{
    public IDiskContainer Open(string filePath, bool readOnly = true)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        
        return extension switch
        {
            ".d88" or ".d77" => new D88DiskContainer(filePath, readOnly),
            ".2d" or ".dsk" => new RawDiskContainer(filePath, readOnly),
            _ => throw new NotSupportedException($"Unsupported disk image format: {extension}")
        };
    }

    public IDiskContainer Create(string filePath, DiskType diskType, string diskName = "", int? sectorsPerTrack = null, ushort? sectorSize = null)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        
        return extension switch
        {
            ".d88" or ".d77" => D88DiskContainer.CreateNew(filePath, diskType, diskName, sectorsPerTrack, sectorSize),
            ".2d" or ".dsk" => RawDiskContainer.CreateNew(filePath, diskType),
            _ => throw new NotSupportedException($"Unsupported disk image format for creation: {extension}")
        };
    }
}
