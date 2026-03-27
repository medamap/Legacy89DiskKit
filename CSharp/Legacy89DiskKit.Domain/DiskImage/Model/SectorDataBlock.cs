namespace Legacy89DiskKit.Domain.DiskImage.Model;

public sealed record SectorDataBlock(
    SectorInfo Sector,
    byte[] Data
);
