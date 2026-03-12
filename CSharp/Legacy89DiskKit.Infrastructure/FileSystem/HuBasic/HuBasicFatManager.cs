using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public class HuBasicFatManager
{
    private readonly IDiskContainer _diskContainer;
    private readonly HuBasicConfiguration _config;

    public HuBasicFatManager(IDiskContainer diskContainer, HuBasicConfiguration config)
    {
        _diskContainer = diskContainer;
        _config = config;
    }

    public byte[] ReadFat()
    {
        var fatData = new byte[_config.FatSectors * _config.SectorSize];
        int startRecord = (_config.FatTrack * _config.SectorsPerTrack) + (_config.FatSector - 1);
        for (int sector = 0; sector < _config.FatSectors; sector++)
        {
            var (c, h, s) = GetPhysicalAddressFromRecord(startRecord + sector);
            var sectorData = _diskContainer.ReadSector(c, h, s);
            Array.Copy(sectorData, 0, fatData, sector * _config.SectorSize, _config.SectorSize);
        }
        return fatData;
    }

    public void WriteFat(byte[] fatData)
    {
        int startRecord = (_config.FatTrack * _config.SectorsPerTrack) + (_config.FatSector - 1);
        for (int sector = 0; sector < _config.FatSectors; sector++)
        {
            var (c, h, s) = GetPhysicalAddressFromRecord(startRecord + sector);
            var sectorData = new byte[_config.SectorSize];
            Array.Copy(fatData, sector * _config.SectorSize, sectorData, 0, _config.SectorSize);
            _diskContainer.WriteSector(c, h, s, sectorData);
        }
    }

    public int GetFatEntry(byte[] fatData, int cluster)
    {
        if (cluster < 0 || cluster >= fatData.Length) return 0x8F; // Terminal/Out of range
        return fatData[cluster];
    }

    public void SetFatEntry(byte[] fatData, int cluster, int value)
    {
        if (cluster >= 0 && cluster < fatData.Length)
        {
            fatData[cluster] = (byte)value;
        }
    }

    public (List<int> Chain, int TerminalFlag) GetClusterChainWithTerminal(int startCluster)
    {
        var chain = new List<int>();
        var fatData = ReadFat();
        var current = startCluster;
        var visited = new HashSet<int>();
        int terminalFlag = 0xFF;

        while (current >= _config.ReservedClusters && current < _config.TotalClusters)
        {
            if (visited.Contains(current)) break; // Circular reference
            visited.Add(current);
            chain.Add(current);

            var next = GetFatEntry(fatData, current);
            if ((next >= 0x80 && next <= 0x8F) || next == 0xFF)
            {
                terminalFlag = next;
                break;
            }
            current = next;
        }
        return (chain, terminalFlag);
    }

    public List<int> GetClusterChain(int startCluster) => GetClusterChainWithTerminal(startCluster).Chain;

    private (int cylinder, int head, int sector) GetPhysicalAddressFromRecord(int recordNumber)
    {
        int cylinder = (recordNumber / _config.SectorsPerTrack) / 2;
        int head = (recordNumber / _config.SectorsPerTrack) % 2;
        int sectorNum = (recordNumber % _config.SectorsPerTrack) + 1;
        return (cylinder, head, sectorNum);
    }
}
