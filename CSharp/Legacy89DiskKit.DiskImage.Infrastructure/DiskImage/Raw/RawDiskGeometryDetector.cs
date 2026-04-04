using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Raw;

public static class RawDiskGeometryDetector
{
    public static RawDiskGeometry Detect(long size)
    {
        return size switch
        {
            327680 => new RawDiskGeometry(40, 2, 16, 256, DiskType.TwoD),
            655360 => new RawDiskGeometry(80, 2, 16, 256, DiskType.TwoDD),
            737280 => new RawDiskGeometry(80, 2, 9, 512, DiskType.TwoDD),
            1261568 => new RawDiskGeometry(77, 2, 8, 1024, DiskType.TwoHD),
            1474560 => new RawDiskGeometry(80, 2, 18, 512, DiskType.TwoHD),
            _ => new RawDiskGeometry(40, 2, 16, 256, DiskType.TwoD)
        };
    }
}
