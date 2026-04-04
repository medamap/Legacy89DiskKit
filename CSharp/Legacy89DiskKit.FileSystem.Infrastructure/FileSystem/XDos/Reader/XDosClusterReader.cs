using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Model.XDos;

namespace Legacy89DiskKit.FileSystem.Infrastructure.XDos.Reader;

public class XDosClusterReader
{
    private readonly IDiskContainer _container;
    private readonly XDosFamReader  _famReader;

    public XDosClusterReader(IDiskContainer container, XDosFamReader famReader)
    {
        _container = container;
        _famReader = famReader;
    }

    public byte[] ReadFile(XDosDirectoryEntry entry)
    {
        if (entry.FileSize <= 0 || entry.FamPointer.Track == 0) return Array.Empty<byte>();

        var famEntries = _famReader.ReadFam(entry.FamPointer);
        var result     = new List<byte>();

        foreach (var fam in famEntries)
        {
            int c = fam.Track / 2;
            int h = fam.Track % 2;
            for (int s = fam.Sector; s < fam.Sector + fam.RecordCount && result.Count < entry.FileSize; s++)
            {
                var sector = _container.ReadSector(c, h, s);
                int take   = Math.Min(sector.Length, entry.FileSize - result.Count);
                result.AddRange(take == sector.Length ? sector : sector[..take]);
            }
        }

        return result.ToArray();
    }
}
