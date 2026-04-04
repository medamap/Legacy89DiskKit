using Legacy89DiskKit.Fdc.Domain.Interface;
using Legacy89DiskKit.DiskImage.Infrastructure.Container;

namespace Legacy89DiskKit.Fdc.Infrastructure.Medium;

public class RawDiskBackedControllerFacingMedium : SectorBackedControllerFacingMedium
{
    private readonly RawDiskContainer _container;

    public RawDiskBackedControllerFacingMedium(RawDiskContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public override string MediumKind => "raw-sector-image";

    public override bool IsWriteProtected => _container.IsReadOnly;

    protected override bool SectorExistsCore(int track, int side, int sector)
    {
        return _container.SectorExists(track, side, sector);
    }

    protected override byte[] ReadSectorCore(int track, int side, int sector)
    {
        return _container.ReadSector(track, side, sector);
    }
}
