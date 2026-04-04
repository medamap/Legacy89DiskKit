using Legacy89DiskKit.DiskImage.Domain.Model;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Models;

public record N88BasicConfiguration(
    int SystemTrack,
    int SystemHead,
    int DirectorySector,
    int DirectorySectors,
    int FatSector,
    int FatSectors,
    int IdSector,
    int SectorSize,
    int ClusterSize,
    int TotalClusters,
    int ReservedClusters,
    int SectorsPerTrack
)
{
    public static N88BasicConfiguration GetDefault(DiskType diskType)
    {
        return diskType switch
        {
            DiskType.TwoD => new N88BasicConfiguration(
                SystemTrack: 18, 
                SystemHead: 1, 
                DirectorySector: 1, 
                DirectorySectors: 12, 
                FatSector: 14, 
                FatSectors: 3, 
                IdSector: 13, 
                SectorSize: 256, 
                ClusterSize: 2048, 
                TotalClusters: 160, // 40 tracks * 2 sides * 2 clusters/track (8 sectors per cluster)
                ReservedClusters: 0, 
                SectorsPerTrack: 16),
            DiskType.TwoDD => new N88BasicConfiguration(
                SystemTrack: 40, 
                SystemHead: 0, 
                DirectorySector: 1, 
                DirectorySectors: 12, 
                FatSector: 14, 
                FatSectors: 3, 
                IdSector: 13, 
                SectorSize: 256, 
                ClusterSize: 4096, 
                TotalClusters: 160, 
                ReservedClusters: 0, 
                SectorsPerTrack: 16),
            _ => throw new NotSupportedException($"DiskType {diskType} is not supported for N88-BASIC yet.")
        };
    }
}
