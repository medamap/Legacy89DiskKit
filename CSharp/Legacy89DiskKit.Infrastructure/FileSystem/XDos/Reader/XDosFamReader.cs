using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFamReader
{
    private readonly byte[] _fam;
    public XDosFamReader(IDiskContainer container) => _fam = container.ReadSector(1, 0, 1);
    public byte[] Fam => _fam;
    public IReadOnlyList<byte> GetChain(byte firstCluster)
    {
        var chain = new List<byte>();
        byte current = firstCluster;
        int guard = 0;
        while (current != 0x00 && current != 0xFF && current != 0xD5 && guard++ < 256)
        {
            chain.Add(current);
            current = _fam[current];
        }
        return chain;
    }
}
