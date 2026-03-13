using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Domain.Fdc.Interface;

namespace Legacy89DiskKit.Application.Drive;

public sealed record MountedMediumBinding(
    IMountedMedium MountedMedium,
    ISectorAddressableMedium? SectorMedium,
    IControllerFacingMedium? ControllerFacingMedium
);
