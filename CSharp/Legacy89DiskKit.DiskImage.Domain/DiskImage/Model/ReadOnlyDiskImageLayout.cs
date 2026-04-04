namespace Legacy89DiskKit.DiskImage.Domain.Model;

public sealed record ReadOnlyDiskImageLayout(
    DiskContainerMetadata Metadata,
    IReadOnlyList<SectorDataBlock> Sectors
);
