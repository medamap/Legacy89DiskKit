using Legacy89DiskKit.DiskImage.Domain.Model;

namespace Legacy89DiskKit.DiskImage.Infrastructure.Raw;

public sealed record RawDiskGeometry(
    int Cylinders,
    int Sides,
    int SectorsPerTrack,
    int BytesPerSector,
    DiskType DiskType
)
{
    public DiskGeometryInfo ToDiskGeometryInfo() => new(
        Cylinders,
        Sides,
        SectorsPerTrack,
        BytesPerSector);
}
