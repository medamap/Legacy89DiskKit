using Legacy89DiskKit.Domain.Drive.Model;

namespace Legacy89DiskKit.Domain.Drive.Interface;

/// <summary>
/// Represents a drive-level contract for controller-facing interaction.
/// </summary>
public interface IFloppyDrive
{
    /// <summary>
    /// Gets the drive number visible to the controller.
    /// </summary>
    int DriveNumber { get; }

    /// <summary>
    /// Returns the current visible drive state.
    /// </summary>
    DriveState GetState();
}
