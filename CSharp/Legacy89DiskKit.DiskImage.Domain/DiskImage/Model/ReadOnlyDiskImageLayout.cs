namespace Legacy89DiskKit.Domain.DiskImage.Model;

public sealed record ReadOnlyDiskImageLayout(
    DiskContainerMetadata Metadata,
    IReadOnlyList<SectorDataBlock> Sectors
);
