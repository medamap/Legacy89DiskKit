using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFatWriter
{
    private readonly IDiskContainer _container;
    private readonly byte[] _fat;
    public XDosFatWriter(IDiskContainer container)
    {
        _container = container;
        _fat = container.ReadSector(0, 1, 1);
    }
    public byte[] Fat => _fat;
    public List<byte> AllocateClusters(int requiredSectors)
    {
        int requiredClusters = Math.Max(1, (requiredSectors + 9) / 10);
        var allocated = new List<byte>();
        for (int i = 3; i < _fat.Length && allocated.Count < requiredClusters; i++)
        {
            if (_fat[i] == 0x00) { _fat[i] = 0x4A; allocated.Add((byte)i); }
        }
        if (allocated.Count < requiredClusters) throw new Exception("Disk full.");
        return allocated;
    }
    public void Commit() => _container.WriteSector(0, 1, 1, _fat);
    public void ClearAll() { Array.Fill(_fat, (byte)0x00); _fat[1] = 0x01; _fat[2] = 0x4A; }
}
