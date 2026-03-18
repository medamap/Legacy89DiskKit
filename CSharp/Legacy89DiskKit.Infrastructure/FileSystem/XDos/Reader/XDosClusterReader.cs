using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosClusterReader
{
    private const int SectorsPerTrack = 10;
    private readonly IDiskContainer _container;
    private readonly XDosFamReader  _fam;

    public XDosClusterReader(IDiskContainer container, XDosFamReader fam)
    {
        _container = container;
        _fam       = fam;
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
            
            // Track 1 (FAT) and Track 2 (FAM) always reserve Sector 1.
            // All other tracks (0, 3+) can use Sector 1 for data.
            int trackStartR = (track == 1 || track == 2) ? 2 : 1;
            int startR = (track == entry.FirstCluster) ? Math.Max(trackStartR, (int)entry.FirstSectorR) : trackStartR;
            
            int maxR = (c == 0 && h == 0) ? 16 : SectorsPerTrack;

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
