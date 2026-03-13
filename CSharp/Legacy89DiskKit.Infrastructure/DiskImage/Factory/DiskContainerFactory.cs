using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Interface.Factory;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Factory;

public class DiskContainerFactory : IDiskContainerFactory
{
    public IDiskContainer Open(string filePath, bool readOnly = true)
    {
        return OpenByFormat(Path.GetExtension(filePath), readOnly, pathFactory: () => filePath, bufferFactory: null);
    }

    public IDiskContainer Open(byte[] imageData, string imageFormat, bool readOnly = true)
    {
        ArgumentNullException.ThrowIfNull(imageData);
        return OpenByFormat(imageFormat, readOnly, pathFactory: null, bufferFactory: () => imageData);
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

    private static IDiskContainer OpenByFormat(string? imageFormat, bool readOnly, Func<string>? pathFactory, Func<byte[]>? bufferFactory)
    {
        var normalized = NormalizeFormat(imageFormat);

        return normalized switch
        {
            ".d88" or ".d77" when pathFactory is not null => new D88DiskContainer(pathFactory(), readOnly),
            ".d88" or ".d77" when bufferFactory is not null => new D88DiskContainer(bufferFactory(), readOnly),
            ".2d" or ".dsk" when pathFactory is not null => new RawDiskContainer(pathFactory(), readOnly),
            ".2d" or ".dsk" when bufferFactory is not null => new RawDiskContainer(bufferFactory(), readOnly),
            _ => throw new NotSupportedException($"Unsupported disk image format: {imageFormat}")
        };
    }

    private static string NormalizeFormat(string? imageFormat)
    {
        if (string.IsNullOrWhiteSpace(imageFormat))
        {
            throw new ArgumentException("Image format must be specified.", nameof(imageFormat));
        }

        var normalized = imageFormat.Trim().ToLowerInvariant();
        return normalized.StartsWith('.') ? normalized : $".{normalized}";
    }
}
