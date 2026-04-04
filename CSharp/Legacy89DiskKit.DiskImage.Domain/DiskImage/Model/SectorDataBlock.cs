namespace Legacy89DiskKit.DiskImage.Domain.Model;

public sealed record SectorDataBlock(
    SectorInfo Sector,
    byte[] Data
);
