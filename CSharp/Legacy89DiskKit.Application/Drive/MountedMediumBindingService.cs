using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.Fdc.Interface;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.Drive.Medium;
using Legacy89DiskKit.Infrastructure.Fdc;
using Legacy89DiskKit.Infrastructure.Fdc.Medium;

namespace Legacy89DiskKit.Application.Drive;

public class MountedMediumBindingService
{
    public MountedMediumBinding CreateFromContainer(IDiskContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        return container switch
        {
            D88DiskContainer d88 => CreateD88Binding(d88),
            RawDiskContainer raw => CreateRawDiskBinding(raw),
            _ => throw new NotSupportedException($"Mounted medium binding is not supported for container type '{container.GetType().Name}'.")
        };
    }

    public MountedMediumBinding MountContainer(DriveMountService driveMountService, int driveNumber, IDiskContainer container)
    {
        ArgumentNullException.ThrowIfNull(driveMountService);

        var binding = CreateFromContainer(container);
        driveMountService.Mount(driveNumber, binding.MountedMedium);
        return binding;
    }

    public IFdcController CreateController(MountedMediumBinding binding, int driveNumber)
    {
        ArgumentNullException.ThrowIfNull(binding);

        if (binding.ControllerFacingMedium is null)
        {
            throw new NotSupportedException("The mounted medium does not expose a controller-facing adapter.");
        }

        return new FdcMediumController(binding.ControllerFacingMedium, driveNumber);
    }

    private static MountedMediumBinding CreateD88Binding(D88DiskContainer container)
    {
        var sectorMedium = new D88BackedSectorAddressableMedium(container);
        var controllerMedium = new D88BackedControllerFacingMedium(container);
        return new MountedMediumBinding(sectorMedium, sectorMedium, controllerMedium);
    }

    private static MountedMediumBinding CreateRawDiskBinding(RawDiskContainer container)
    {
        var sectorMedium = new RawDiskBackedSectorAddressableMedium(container);
        var controllerMedium = new RawDiskBackedControllerFacingMedium(container);
        return new MountedMediumBinding(sectorMedium, sectorMedium, controllerMedium);
    }
}
