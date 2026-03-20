using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosClusterReader
{
    private readonly IDiskContainer _container;
    private readonly XDosFamReader  _fam;
    private readonly XDosMediaGeometry _geometry;

    public XDosClusterReader(IDiskContainer container, XDosFamReader fam, XDosMediaGeometry geometry)
    {
        _container = container;
        _fam       = fam;
        _geometry  = geometry;
    }

    public byte[] ReadFile(XDosDirectoryEntry entry)
    {
        if (entry.FileSize <= 0 || entry.FirstCluster == 0) return Array.Empty<byte>();

        var chain = _fam.GetChain(entry.FirstCluster);
        var result = new List<byte>();

        foreach (byte track in chain)
        {
            int c = track / 2;
            int h = track % 2;
            
            int trackStartR = (track == 1 || track == 2) ? 2 : 1;
            int startR = (track == entry.FirstCluster) ? Math.Max(trackStartR, (int)entry.FirstSectorR) : trackStartR;
            
            var (maxR, _, _) = _geometry.GetTrackGeometry(c, h);

            for (int r = startR; r <= maxR && result.Count < entry.FileSize; r++)
            {
                var sector = _container.ReadSector(c, h, r);
                int take = Math.Min(sector.Length, entry.FileSize - result.Count);
                if (take > 0) result.AddRange(take == sector.Length ? sector : sector[..take]);
            }
        }

        return result.ToArray();
    }
}
