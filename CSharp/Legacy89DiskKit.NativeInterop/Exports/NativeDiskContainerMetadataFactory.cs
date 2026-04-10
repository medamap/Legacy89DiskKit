using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Exports;

public static class NativeDiskContainerMetadataFactory
{
    public static NativeDiskContainerMetadata Create(DiskContainerMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return new NativeDiskContainerMetadata
        {
            ImageFormat = metadata.ImageFormat,
            DiskType = (int)metadata.DiskType,
            Cylinders = metadata.Geometry.Cylinders,
            Heads = metadata.Geometry.Heads,
            SectorsPerTrack = metadata.Geometry.SectorsPerTrack,
            BytesPerSector = metadata.Geometry.BytesPerSector,
            IsWriteProtected = metadata.IsWriteProtected ? 1 : 0,
            DeclaredImageSize = metadata.DeclaredImageSize
        };
    }
}
