using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.Drive.Domain.Interface;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;

namespace Legacy89DiskKit.Drive.Infrastructure.Medium;

public class RawDiskBackedSectorAddressableMedium : ISectorAddressableMedium
{
    private readonly RawDiskContainer _container;

    public RawDiskBackedSectorAddressableMedium(RawDiskContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public string MediumKind => "raw-sector-image";

    public bool SupportsDirectImageAccess => true;

    public bool SupportsControllerFacingAccess => true;

    public bool SectorExists(int cylinder, int head, int sector)
    {
        return _container.SectorExists(cylinder, head, sector);
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted = false)
    {
        return _container.ReadSector(cylinder, head, sector, allowCorrupted);
    }

    public IEnumerable<SectorInfo> GetAllSectors()
    {
        return _container.GetAllSectors();
    }
}
