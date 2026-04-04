namespace Legacy89DiskKit.DiskImage.Domain.Model;

public sealed record DiskGeometryInfo(
    int Cylinders,
    int Heads,
    int SectorsPerTrack,
    int BytesPerSector
);
