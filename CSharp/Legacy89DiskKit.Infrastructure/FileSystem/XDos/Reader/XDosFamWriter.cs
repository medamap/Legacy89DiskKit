using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFamWriter
{
    private readonly IDiskContainer _container;
    private readonly byte[] _fam;
    public XDosFamWriter(IDiskContainer container)
    {
        _container = container;
        _fam = container.ReadSector(1, 0, 1);
    }
    public byte[] Fam => _fam;
    public void UpdateChain(List<byte> clusters)
    {
        for (int i = 0; i < clusters.Count - 1; i++) _fam[clusters[i]] = clusters[i + 1];
        if (clusters.Count > 0) _fam[clusters[^1]] = 0x00;
    }
    public void Commit() => _container.WriteSector(1, 0, 1, _fam);
    public void ClearAll() => Array.Fill(_fam, (byte)0x00);
}
