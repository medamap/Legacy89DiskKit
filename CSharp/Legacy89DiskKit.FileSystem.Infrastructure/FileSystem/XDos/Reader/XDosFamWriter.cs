using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFamWriter
{
    private readonly IDiskContainer _container;

    public XDosFamWriter(IDiskContainer container) => _container = container;

    public void WriteFam(int famTrack, int famSector, IReadOnlyList<XDosFamEntry> entries)
    {
        int c = famTrack / 2;
        int h = famTrack % 2;
        var sector = new byte[512];
        int i = 0;
        foreach (var e in entries)
        {
            if (i + 2 >= sector.Length) break;
            sector[i++] = e.Track;
            sector[i++] = e.Sector;
            sector[i++] = e.RecordCount;
        }
        sector[i] = 0x00;
        _container.WriteSector(c, h, famSector, sector);
    }

    public void ClearFam(int famTrack, int famSector)
    {
        int c = famTrack / 2;
        int h = famTrack % 2;
        _container.WriteSector(c, h, famSector, new byte[512]);
    }

    public void ClearAll() { }
}
