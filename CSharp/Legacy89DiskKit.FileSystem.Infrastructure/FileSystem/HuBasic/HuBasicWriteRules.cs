using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public static class HuBasicWriteRules
{
    public static byte[] PrepareWritePayload(byte[] data, ExtendedFileAttributes attributes)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (attributes == null) throw new ArgumentNullException(nameof(attributes));

        if (!attributes.IsAscii)
        {
            return data;
        }

        if (data.Length > 0 && data[^1] == 0x1A)
        {
            return data;
        }

        var newData = new byte[data.Length + 1];
        Array.Copy(data, newData, data.Length);
        newData[^1] = 0x1A;
        return newData;
    }

    public static int GetClustersNeeded(int dataLength, HuBasicConfiguration config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        int clustersNeeded = (dataLength + config.ClusterSize - 1) / config.ClusterSize;
        return clustersNeeded == 0 ? 1 : clustersNeeded;
    }

    public static int GetSectorsInLastCluster(int dataLength, HuBasicConfiguration config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        int sectorsPerCluster = config.ClusterSize / config.SectorSize;
        int sectorsInLastCluster = ((dataLength + config.SectorSize - 1) / config.SectorSize) % sectorsPerCluster;
        return sectorsInLastCluster == 0 ? sectorsPerCluster : sectorsInLastCluster;
    }

    public static int GetTerminalFlagForLength(int dataLength, HuBasicConfiguration config)
    {
        return 0x7F + GetSectorsInLastCluster(dataLength, config);
    }
}
