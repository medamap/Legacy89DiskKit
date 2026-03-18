using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFatReader
{
    private readonly byte[] _fat;

    public XDosFatReader(IDiskContainer container)
    {
        _fat = container.ReadSector(1, 0, 1);
    }

    public bool IsClusterFree(int clusterIndex) => _fat[clusterIndex] == 0x00;
    public int CountFreeClusters() => _fat.Count(b => b == 0x00);
    public int CountUsedClusters() => _fat.Count(b => b == 0x4A);
}
