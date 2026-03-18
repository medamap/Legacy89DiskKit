using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFamReader
{
    private readonly byte[] _fam;
    private const int MaxChainLength = 256;

    public XDosFamReader(IDiskContainer container)
    {
        _fam = container.ReadSector(2, 0, 1);
    }

    public IReadOnlyList<byte> GetChain(byte firstCluster)
    {
        var chain = new List<byte>();
        byte current = firstCluster;
        int guard = 0;
        while (current != 0x00 && guard++ < MaxChainLength)
        {
            chain.Add(current);
            current = _fam[current];
        }
        return chain;
    }
}
