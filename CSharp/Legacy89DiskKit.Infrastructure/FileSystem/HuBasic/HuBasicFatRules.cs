using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public static class HuBasicFatRules
{
    public static int GetEntry(byte[] fatData, int cluster)
    {
        if (fatData == null) throw new ArgumentNullException(nameof(fatData));
        if (cluster < 0 || cluster >= fatData.Length) return 0x8F;
        return fatData[cluster];
    }

    public static void SetEntry(byte[] fatData, int cluster, int value)
    {
        if (fatData == null) throw new ArgumentNullException(nameof(fatData));
        if (cluster >= 0 && cluster < fatData.Length)
        {
            fatData[cluster] = (byte)value;
        }
    }

    public static bool IsTerminal(int value)
    {
        return (value >= 0x80 && value <= 0x8F) || value == 0xFF;
    }

    public static int GetLastClusterUsedSectors(int terminalFlag)
    {
        if (terminalFlag < 0x80 || terminalFlag > 0x8F)
        {
            return 0;
        }

        return terminalFlag - 0x7F;
    }

    public static HuBasicFatChainResult GetClusterChain(byte[] fatData, HuBasicConfiguration config, int startCluster)
    {
        if (fatData == null) throw new ArgumentNullException(nameof(fatData));
        if (config == null) throw new ArgumentNullException(nameof(config));

        var chain = new List<int>();
        var current = startCluster;
        var visited = new HashSet<int>();
        int terminalFlag = 0xFF;

        while (current >= config.ReservedClusters && current < config.TotalClusters)
        {
            if (visited.Contains(current))
            {
                break;
            }

            visited.Add(current);
            chain.Add(current);

            var next = GetEntry(fatData, current);
            if (IsTerminal(next))
            {
                terminalFlag = next;
                break;
            }

            current = next;
        }

        return new HuBasicFatChainResult(chain, terminalFlag);
    }

    public static void ApplyChain(byte[] fatData, IReadOnlyList<int> clusters, int terminalFlag)
    {
        if (fatData == null) throw new ArgumentNullException(nameof(fatData));
        if (clusters == null) throw new ArgumentNullException(nameof(clusters));

        for (int i = 0; i < clusters.Count; i++)
        {
            int next = i == clusters.Count - 1 ? terminalFlag : clusters[i + 1];
            SetEntry(fatData, clusters[i], next);
        }
    }
}
