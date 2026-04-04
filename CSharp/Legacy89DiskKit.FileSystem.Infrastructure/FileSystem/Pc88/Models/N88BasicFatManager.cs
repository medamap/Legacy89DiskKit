using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Models;

public class N88BasicFatManager
{
    private readonly IDiskContainer _diskContainer;
    private readonly N88BasicConfiguration _config;

    public N88BasicFatManager(IDiskContainer diskContainer, N88BasicConfiguration config)
    {
        _diskContainer = diskContainer;
        _config = config;
    }

    public byte[] ReadFat()
    {
        // FAT is usually 3 sectors in N88-BASIC (Track 18/Side 1/S14-16)
        // We read the first copy.
        var data = new byte[_config.FatSectors * _config.SectorSize];
        for (int i = 0; i < _config.FatSectors; i++)
        {
            var sectorData = _diskContainer.ReadSector(_config.SystemTrack, _config.SystemHead, _config.FatSector + i);
            Array.Copy(sectorData, 0, data, i * _config.SectorSize, _config.SectorSize);
        }
        return data;
    }

    public void WriteFat(byte[] fatData)
    {
        // Write to all 3 copies if they are standard (S14, S15, S16 in 2D)
        // For now, we write to the primary copy.
        for (int i = 0; i < _config.FatSectors; i++)
        {
            var sectorData = new byte[_config.SectorSize];
            Array.Copy(fatData, i * _config.SectorSize, sectorData, 0, _config.SectorSize);
            _diskContainer.WriteSector(_config.SystemTrack, _config.SystemHead, _config.FatSector + i, sectorData);
        }
    }

    public byte GetFatEntry(byte[] fatData, int cluster)
    {
        if (cluster < 0 || cluster >= fatData.Length) return 0xFE; // Reserved/Out of bounds
        return fatData[cluster];
    }

    public void SetFatEntry(byte[] fatData, int cluster, int value)
    {
        if (cluster >= 0 && cluster < fatData.Length)
        {
            fatData[cluster] = (byte)value;
        }
    }

    public List<int> GetClusterChain(int startCluster)
    {
        var chain = new List<int>();
        var fat = ReadFat();
        int current = startCluster;

        // Safety limit to prevent infinite loops (Max clusters in a disk)
        int safetyLimit = _config.TotalClusters;
        
        while (safetyLimit-- > 0)
        {
            if (current == 0xFF || current == 0xFE) break; // Unused or System
            chain.Add(current);
            byte entry = GetFatEntry(fat, current);
            
            // Check for EOF: C0h + sectors (C0-CF)
            if (entry >= 0xC0 && entry <= 0xCF) break;
            
            // Checks for out of range
            if (entry >= _config.TotalClusters) break;

            current = entry;
        }

        return chain;
    }
}
