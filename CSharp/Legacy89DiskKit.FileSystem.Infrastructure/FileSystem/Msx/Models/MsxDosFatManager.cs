using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Msx.Models;

public class MsxDosFatManager
{
    private readonly IDiskContainer _diskContainer;
    private readonly int _fatStartSector;
    private readonly int _fatSize;

    public MsxDosFatManager(IDiskContainer diskContainer, int fatStartSector, int fatSize)
    {
        _diskContainer = diskContainer;
        _fatStartSector = fatStartSector;
        _fatSize = fatSize;
    }

    public byte[] ReadFat()
    {
        using var ms = new MemoryStream();
        for (int i = 0; i < _fatSize; i++)
        {
            // MSX DSK files use logical sectors. 
            // In our current architecture, we might need to map logical sector to C/H/S.
            // For raw DSK, LBA 0 is C0/H0/S1.
            var (c, h, s) = LbaToPhysical(_fatStartSector + i);
            ms.Write(_diskContainer.ReadSector(c, h, s));
        }
        return ms.ToArray();
    }
    public void WriteFat(byte[] fatData)
    {
        // MSX usually has 2 FAT copies.
        for (int f = 0; f < 2; f++) 
        {
            int offset = f * _fatSize;
            for (int i = 0; i < _fatSize; i++)
            {
                var sectorData = new byte[512]; // MSX default sector size
                Array.Copy(fatData, i * 512, sectorData, 0, 512);
                var (c, h, s) = LbaToPhysical(_fatStartSector + offset + i);
                _diskContainer.WriteSector(c, h, s, sectorData);
            }
        }
    }

    public ushort GetFatEntry(byte[] fat, int cluster)
    {
        int offset = (cluster * 3) / 2;
        if (offset + 1 >= fat.Length) return 0xFFF;

        if (cluster % 2 == 0)
        {
            return (ushort)(((fat[offset + 1] & 0x0F) << 8) | fat[offset]);
        }
        else
        {
            // For odd cluster: high 4 bits of fat[offset] and all bits of fat[offset + 1]
            return (ushort)((fat[offset + 1] << 4) | ((fat[offset] & 0xF0) >> 4));
        }
    }

    public void SetFatEntry(byte[] fat, int cluster, ushort value)
    {
        int offset = (cluster * 3) / 2;
        if (offset + 1 >= fat.Length) return;

        if (cluster % 2 == 0)
        {
            fat[offset] = (byte)(value & 0xFF);
            fat[offset + 1] = (byte)((fat[offset + 1] & 0xF0) | ((value >> 8) & 0x0F));
        }
        else
        {
            fat[offset] = (byte)((fat[offset] & 0x0F) | ((value << 4) & 0xF0));
            fat[offset + 1] = (byte)((value >> 4) & 0xFF);
        }
    }

    public List<int> GetClusterChain(int startCluster)
    {
        var chain = new List<int>();
        if (startCluster < 2 || startCluster > 0xFF0) return chain;

        var fat = ReadFat();
        int current = startCluster;
        while (current >= 0x002 && current <= 0xFEF)
        {
            chain.Add(current);
            current = GetFatEntry(fat, current);
            if (chain.Count > 4096) break; // Safety
        }
        if (current >= 0xFF8) // EOF
        {
            // Already added clusters
        }
        return chain;
    }

    private (int c, int h, int s) LbaToPhysical(int lba)
    {
        // MSX 720KB: 2 sides, 9 sectors/track, 80 tracks.
        int trackSize = 9;
        int cylinder = lba / (trackSize * 2);
        int head = (lba / trackSize) % 2;
        int sector = (lba % trackSize) + 1;
        return (cylinder, head, sector);
    }
}
