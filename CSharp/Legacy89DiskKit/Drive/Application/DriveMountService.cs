using Legacy89DiskKit.Domain.Drive.Interface;
using Legacy89DiskKit.Domain.Drive.Model;

namespace Legacy89DiskKit.Drive.Application;

public class DriveMountService
{
    private readonly Dictionary<int, IMountedMedium> _mountedMedia = new();

    public void Mount(int driveNumber, IMountedMedium medium)
    {
        ArgumentNullException.ThrowIfNull(medium);

        if (driveNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(driveNumber), "Drive number must be zero or greater.");
        }

        _mountedMedia[driveNumber] = medium;
    }

    public bool Unmount(int driveNumber)
    {
        return _mountedMedia.Remove(driveNumber);
    }

    public bool IsMounted(int driveNumber)
    {
        return _mountedMedia.ContainsKey(driveNumber);
    }

    public IMountedMedium? GetMountedMedium(int driveNumber)
    {
        return _mountedMedia.GetValueOrDefault(driveNumber);
    }

    public DriveState GetState(int driveNumber, int currentTrack = 0, int selectedSide = 0, bool motorOn = false, bool isWriteProtected = false)
    {
        var medium = GetMountedMedium(driveNumber);
        return new DriveState(
            driveNumber,
            medium is not null,
            currentTrack,
            selectedSide,
            motorOn,
            medium is not null,
            isWriteProtected,
            medium?.MediumKind
        );
    }
}
