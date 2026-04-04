using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;

namespace Legacy89DiskKit.Infrastructure.Drive.Medium;

public class D88BackedSectorAddressableMedium : ISectorAddressableMedium
{
    private readonly D88DiskContainer _container;

    public D88BackedSectorAddressableMedium(D88DiskContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public string MediumKind => "d88-family";

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
