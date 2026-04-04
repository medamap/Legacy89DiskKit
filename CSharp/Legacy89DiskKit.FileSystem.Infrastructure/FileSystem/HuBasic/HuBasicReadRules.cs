using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Model;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;

namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;

public static class HuBasicReadRules
{
    public static byte[] TrimToRecordedLength(byte[] data, FileEntry file)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (file == null) throw new ArgumentNullException(nameof(file));

        return file.Size > 0 && data.Length > file.Size
            ? data.Take((int)file.Size).ToArray()
            : data;
    }

    public static byte[] TrimToTerminalLength(byte[] data, DiskType diskType, HuBasicConfiguration config, int clusterCount, int terminalFlag, long recordedSize)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (config == null) throw new ArgumentNullException(nameof(config));

        if (diskType != DiskType.TwoHD)
        {
            return data;
        }

        int usedInLast = HuBasicFatRules.GetLastClusterUsedSectors(terminalFlag);
        if (usedInLast == 0)
        {
            return data;
        }

        int sectorsPerCluster = config.ClusterSize / config.SectorSize;
        int totalRecords = (clusterCount - 1) * sectorsPerCluster + usedInLast;
        int totalBytes = totalRecords * config.SectorSize;

        if (recordedSize == 0 || totalBytes < data.Length)
        {
            return data.Take(totalBytes).ToArray();
        }

        return data;
    }

    public static byte[] ExtractAsciiPayload(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var result = new List<byte>(data.Length);
        foreach (byte b in data)
        {
            if (b == 0x1A)
            {
                break;
            }

            result.Add(b);
        }

        return result.ToArray();
    }

    public static byte[] ResolveReadPayload(byte[] data, FileEntry file, DiskType diskType, HuBasicConfiguration config, int clusterCount, int terminalFlag)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (config == null) throw new ArgumentNullException(nameof(config));

        byte[] adjusted = TrimToTerminalLength(data, diskType, config, clusterCount, terminalFlag, file.Size);

        if (file.Attributes.IsAscii)
        {
            return ExtractAsciiPayload(adjusted);
        }

        return TrimToRecordedLength(adjusted, file);
    }
}
