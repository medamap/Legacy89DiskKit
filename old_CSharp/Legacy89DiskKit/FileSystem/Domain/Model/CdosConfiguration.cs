namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// Represents CDOS file system configuration for different disk types
/// </summary>
public class CdosConfiguration
{
    /// <summary>
    /// Gets the disk type
    /// </summary>
    public DiskImage.Domain.Interface.Container.DiskType DiskType { get; init; }

    /// <summary>
    /// Gets the number of tracks
    /// </summary>
    public int TrackCount { get; init; }

    /// <summary>
    /// Gets the number of heads
    /// </summary>
    public int HeadCount { get; init; }

    /// <summary>
    /// Gets the sectors per track
    /// </summary>
    public int SectorsPerTrack { get; init; }

    /// <summary>
    /// Gets the sector size
    /// </summary>
    public int SectorSize { get; init; }

    /// <summary>
    /// Gets the directory start track
    /// </summary>
    public int DirectoryStartTrack { get; init; }

    /// <summary>
    /// Gets the directory start sector
    /// </summary>
    public int DirectoryStartSector { get; init; }

    /// <summary>
    /// Gets the maximum number of directory entries
    /// </summary>
    public int MaxDirectoryEntries { get; init; }

    /// <summary>
    /// Gets whether this is a mixed sector size format (2HD)
    /// </summary>
    public bool IsMixedSectorSize { get; init; }

    /// <summary>
    /// Gets the track 0 sector size (for mixed formats)
    /// </summary>
    public int Track0SectorSize { get; init; }

    /// <summary>
    /// Gets the track 0 sectors per track (for mixed formats)
    /// </summary>
    public int Track0SectorsPerTrack { get; init; }

    /// <summary>
    /// Gets the configuration for the specified disk type
    /// </summary>
    public static CdosConfiguration GetConfiguration(DiskImage.Domain.Interface.Container.DiskType diskType)
    {
        return diskType switch
        {
            DiskImage.Domain.Interface.Container.DiskType.TwoD => new CdosConfiguration
            {
                DiskType = diskType,
                TrackCount = 40,
                HeadCount = 2,
                SectorsPerTrack = 16,
                SectorSize = 256,
                DirectoryStartTrack = 1,
                DirectoryStartSector = 1,
                MaxDirectoryEntries = 128,
                IsMixedSectorSize = false,
                Track0SectorSize = 256,
                Track0SectorsPerTrack = 16
            },
            DiskImage.Domain.Interface.Container.DiskType.TwoHD => new CdosConfiguration
            {
                DiskType = diskType,
                TrackCount = 77,
                HeadCount = 2,
                SectorsPerTrack = 8,
                SectorSize = 1024,
                DirectoryStartTrack = 1,
                DirectoryStartSector = 1,
                MaxDirectoryEntries = 128,
                IsMixedSectorSize = true,
                Track0SectorSize = 128,
                Track0SectorsPerTrack = 26
            },
            _ => throw new NotSupportedException($"CDOS does not support disk type: {diskType}")
        };
    }

    /// <summary>
    /// Calculates the total disk capacity
    /// </summary>
    public int GetDiskCapacity()
    {
        if (!IsMixedSectorSize)
        {
            return TrackCount * HeadCount * SectorsPerTrack * SectorSize;
        }

        // Track 0 has different sector size
        var track0Capacity = HeadCount * Track0SectorsPerTrack * Track0SectorSize;
        var otherTracksCapacity = (TrackCount - 1) * HeadCount * SectorsPerTrack * SectorSize;
        return track0Capacity + otherTracksCapacity;
    }
}