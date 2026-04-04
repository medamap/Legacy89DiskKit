namespace Legacy89DiskKit.DiskImage.Infrastructure.D88;

public sealed class D88SectorData
{
    public byte Cylinder { get; init; }

    public byte Head { get; init; }

    public byte Sector { get; init; }

    public byte SectorSizeN { get; init; }

    public ushort SectorCount { get; init; }

    public byte Density { get; init; }

    public bool Deleted { get; set; }

    public byte Status { get; init; }

    public ushort ActualSize { get; set; }

    public byte[] Data { get; set; } = [];
}
