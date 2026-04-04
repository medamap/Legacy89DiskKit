namespace Legacy89DiskKit.DiskImage.Infrastructure.Raw;

public sealed class RawSectorAddressCalculator
{
    private readonly RawDiskGeometry _geometry;

    public RawSectorAddressCalculator(RawDiskGeometry geometry)
    {
        _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
    }

    public bool SectorExists(int cylinder, int head, int sector)
    {
        return cylinder >= 0 && cylinder < _geometry.Cylinders &&
               head >= 0 && head < _geometry.Sides &&
               sector >= 1 && sector <= _geometry.SectorsPerTrack;
    }

    public int CalculateOffset(int cylinder, int head, int sector)
    {
        if (!SectorExists(cylinder, head, sector))
        {
            throw new ArgumentOutOfRangeException(nameof(sector), $"Invalid sector address: C:{cylinder} H:{head} S:{sector}");
        }

        return ((cylinder * _geometry.Sides + head) * _geometry.SectorsPerTrack + (sector - 1)) * _geometry.BytesPerSector;
    }
}
