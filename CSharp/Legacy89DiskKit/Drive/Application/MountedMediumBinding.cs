using Legacy89DiskKit.Drive.Domain.Interface;
using Legacy89DiskKit.Fdc.Domain.Interface;

namespace Legacy89DiskKit.Drive.Application;

public sealed record MountedMediumBinding(
    IMountedMedium MountedMedium,
    ISectorAddressableMedium? SectorMedium,
    IControllerFacingMedium? ControllerFacingMedium
);
