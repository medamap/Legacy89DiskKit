using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;

namespace Legacy89DiskKit.Infrastructure.Fdc.Medium;

public class D88BackedControllerFacingMedium : SectorBackedControllerFacingMedium
{
    private readonly D88DiskContainer _container;

    public D88BackedControllerFacingMedium(D88DiskContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public override string MediumKind => "d88-family";

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
