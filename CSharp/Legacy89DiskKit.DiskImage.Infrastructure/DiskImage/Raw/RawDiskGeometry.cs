using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Infrastructure.DiskImage.Raw;

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
