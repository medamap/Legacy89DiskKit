using Legacy89DiskKit.Domain.DiskImage.Model;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;

public record HuBasicConfiguration
{
    public int TotalTracks { get; init; }
    public int SectorsPerTrack { get; init; }
    public int SectorSize { get; init; }
    public int ClusterSize { get; init; }
    public int TotalClusters { get; init; }
    public int ReservedClusters { get; init; }
    public int FatTrack { get; init; }
    public int FatSector { get; init; }
    public int FatSectors { get; init; }
    public int DirectoryTrack { get; init; }
    public int DirectorySector { get; init; }
    public int DirectorySectors { get; init; }

    public static HuBasicConfiguration GetDefault(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => new HuBasicConfiguration
            {
                TotalTracks = 80,
                SectorsPerTrack = 16,
                SectorSize = 256,
                ClusterSize = 16 * 256,
                TotalClusters = 80,
                ReservedClusters = 2,
                FatTrack = 0,
                FatSector = 15,
                FatSectors = 1,
                DirectoryTrack = 1,
                DirectorySector = 1,
                DirectorySectors = 16
            },
            DiskType.TwoDD => new HuBasicConfiguration
            {
                TotalTracks = 160,
                SectorsPerTrack = 16,
                SectorSize = 256,
                ClusterSize = 16 * 256,
                TotalClusters = 160,
                ReservedClusters = 2,
                FatTrack = 0,
                FatSector = 15,
                FatSectors = 2,
                DirectoryTrack = 1,
                DirectorySector = 1,
                DirectorySectors = 16
            },
            DiskType.TwoHD => new HuBasicConfiguration
            {
                TotalTracks = 154,
                SectorsPerTrack = 26,
                SectorSize = 256,
                ClusterSize = 16 * 256,
                TotalClusters = 250,
                ReservedClusters = 3,
                FatTrack = 1,
                FatSector = 3,
                FatSectors = 2,
                DirectoryTrack = 1,
                DirectorySector = 7,
                DirectorySectors = 16
            },
            _ => throw new ArgumentException($"Unsupported disk type for Hu-BASIC: {diskType}")
        };
    }
}
