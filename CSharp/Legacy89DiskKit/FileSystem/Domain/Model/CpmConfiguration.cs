using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.FileSystem.Domain.Model;

/// <summary>
/// CP/M disk configuration parameters (equivalent to DPB - Disk Parameter Block)
/// </summary>
public class CpmConfiguration
{
    /// <summary>
    /// Gets the size of each allocation block in bytes
    /// </summary>
    public int BlockSize { get; init; }

    /// <summary>
    /// Gets the number of directory entries
    /// </summary>
    public int DirectoryEntries { get; init; }

    /// <summary>
    /// Gets the total number of allocation blocks
    /// </summary>
    public int TotalBlocks { get; init; }

    /// <summary>
    /// Gets the number of reserved tracks for system
    /// </summary>
    public int ReservedTracks { get; init; }

    /// <summary>
    /// Gets the number of sectors per track
    /// </summary>
    public int SectorsPerTrack { get; init; }

    /// <summary>
    /// Gets the sector size in bytes (typically 128)
    /// </summary>
    public int SectorSize { get; init; }

    /// <summary>
    /// Gets the extent mask
    /// </summary>
    public byte ExtentMask { get; init; }

    /// <summary>
    /// Gets the block shift value
    /// </summary>
    public byte BlockShift { get; init; }

    /// <summary>
    /// Gets the block mask value
    /// </summary>
    public byte BlockMask { get; init; }

    /// <summary>
    /// Gets the number of tracks on the disk
    /// </summary>
    public int TrackCount { get; init; }

    /// <summary>
    /// Gets the number of sectors per block
    /// </summary>
    public int SectorsPerBlock => BlockSize / SectorSize;

    /// <summary>
    /// Gets the directory start track
    /// </summary>
    public int DirectoryStartTrack => ReservedTracks;

    /// <summary>
    /// Gets predefined configuration for 2D disk format
    /// </summary>
    public static CpmConfiguration Disk2D => new()
    {
        BlockSize = 1024,
        DirectoryEntries = 64,
        TotalBlocks = 160,
        ReservedTracks = 2,
        SectorsPerTrack = 16,
        SectorSize = 128,
        TrackCount = 40,
        ExtentMask = 0,
        BlockShift = 3,
        BlockMask = 7
    };

    /// <summary>
    /// Gets predefined configuration for 2DD disk format
    /// </summary>
    public static CpmConfiguration Disk2DD => new()
    {
        BlockSize = 2048,
        DirectoryEntries = 128,
        TotalBlocks = 160,
        ReservedTracks = 2,
        SectorsPerTrack = 16,
        SectorSize = 256,
        TrackCount = 40,
        ExtentMask = 1,
        BlockShift = 4,
        BlockMask = 15
    };

    /// <summary>
    /// Gets predefined configuration for 2HD disk format
    /// </summary>
    public static CpmConfiguration Disk2HD => new()
    {
        BlockSize = 2048,
        DirectoryEntries = 256,
        TotalBlocks = 620,
        ReservedTracks = 2,
        SectorsPerTrack = 26,
        SectorSize = 256,
        TrackCount = 77,
        ExtentMask = 1,
        BlockShift = 4,
        BlockMask = 15
    };

    /// <summary>
    /// Gets configuration based on disk type
    /// </summary>
    public static CpmConfiguration GetConfiguration(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => Disk2D,
            DiskType.TwoDD => Disk2DD,
            DiskType.TwoHD => Disk2HD,
            _ => throw new ArgumentException($"Unsupported disk type for CP/M: {diskType}")
        };
    }
}