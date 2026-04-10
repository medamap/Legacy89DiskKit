namespace Legacy89DiskKit.Domain.Drive.Model;

/// <summary>
/// Represents the visible state of a mounted or mountable floppy drive.
/// </summary>
public sealed record DriveState(
    int DriveNumber,
    bool HasMountedMedium,
    int CurrentTrack,
    int SelectedSide,
    bool MotorOn,
    bool IsReady,
    bool IsWriteProtected,
    string? MountedMediumKind = null
);
