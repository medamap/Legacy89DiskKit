using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Raw;

public static class RawDiskImageDescriptor
{
    public static DiskContainerMetadata Describe(byte[] imageData)
    {
        if (imageData is null)
        {
            throw new ArgumentNullException(nameof(imageData));
        }

        var geometry = RawDiskGeometryDetector.Detect(imageData.LongLength);
        return new DiskContainerMetadata(
            ImageFormat: "raw-sector-image",
            DiskType: geometry.DiskType,
            Geometry: geometry.ToDiskGeometryInfo(),
            IsWriteProtected: false,
            DeclaredImageSize: imageData.LongLength);
    }
}
