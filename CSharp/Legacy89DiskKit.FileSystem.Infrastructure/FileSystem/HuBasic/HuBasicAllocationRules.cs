using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public static class HuBasicAllocationRules
{
    public static int GetFatScanLimit(DiskType diskType, HuBasicConfiguration config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        return config.TotalClusters;
    }

    public static bool IsAllocatableCluster(DiskType diskType, HuBasicConfiguration config, int cluster)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (cluster < config.ReservedClusters)
        {
            return false;
        }

        return cluster < GetFatScanLimit(diskType, config);
    }

    public static List<int> CollectFreeClusters(byte[] fatData, DiskType diskType, HuBasicConfiguration config, int count)
    {
        if (fatData == null) throw new ArgumentNullException(nameof(fatData));
        if (config == null) throw new ArgumentNullException(nameof(config));

        var allocated = new List<int>();
        int maxIndex = GetFatScanLimit(diskType, config);

        for (int cluster = config.ReservedClusters; cluster < maxIndex && allocated.Count < count; cluster++)
        {
            if (!IsAllocatableCluster(diskType, config, cluster))
            {
                continue;
            }

            if (HuBasicFatRules.GetEntry(fatData, cluster) == 0)
            {
                allocated.Add(cluster);
            }
        }

        return allocated;
    }
}
