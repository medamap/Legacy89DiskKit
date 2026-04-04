using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Model.XDos;

namespace Legacy89DiskKit.FileSystem.Infrastructure.XDos.Reader;

public record XDosFamEntry(byte Track, byte Sector, byte RecordCount);

public class XDosFamReader
{
    private readonly IDiskContainer _container;

    public XDosFamReader(IDiskContainer container) => _container = container;

    public IReadOnlyList<XDosFamEntry> ReadFam(XDosFamPointer famPointer)
    {
        if (famPointer.Track == 0) return Array.Empty<XDosFamEntry>();
        int c = famPointer.Track / 2;
        int h = famPointer.Track % 2;
        var sector = _container.ReadSector(c, h, famPointer.Sector);
        return ParseFam(sector);
    }

    public static List<XDosFamEntry> ParseFam(byte[] sector)
    {
        var entries = new List<XDosFamEntry>();
        int i = 0;
        while (i + 2 < sector.Length && sector[i] != 0x00)
        {
            entries.Add(new XDosFamEntry(sector[i], sector[i + 1], sector[i + 2]));
            i += 3;
        }
        return entries;
    }
}
