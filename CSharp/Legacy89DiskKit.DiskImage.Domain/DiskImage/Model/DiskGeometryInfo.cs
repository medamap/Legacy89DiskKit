namespace Legacy89DiskKit.Domain.DiskImage.Model;

public sealed record DiskGeometryInfo(
    int Cylinders,
    int Heads,
    int SectorsPerTrack,
    int BytesPerSector
);
