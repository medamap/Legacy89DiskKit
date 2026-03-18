using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosClusterReader
{
    private const int SectorsPerTrack = 10;
    private const int SectorSize      = 512;

    private readonly IDiskContainer _container;
    private readonly XDosFamReader  _fam;

    public XDosClusterReader(IDiskContainer container, XDosFamReader fam)
    {
        _container = container;
        _fam       = fam;
    }

    public byte[] ReadFile(XDosDirectoryEntry entry)
    {
        var chain = _fam.GetChain(entry.FirstCluster);
        int targetSize = entry.FileSize > 0 ? entry.FileSize : int.MaxValue;
        var result = new List<byte>();

        for (int i = 0; i < chain.Count && result.Count < targetSize; i++)
        {
            byte cluster = chain[i];
            int startR   = (i == 0) ? entry.FirstSectorR : 1;
            // TODO: confirm cluster-to-physical-track formula
            int physicalTrack = cluster;

            for (int r = startR; r <= SectorsPerTrack && result.Count < targetSize; r++)
            {
                var sector = _container.ReadSector(physicalTrack, 0, r);
                int take = Math.Min(sector.Length, targetSize - result.Count);
                result.AddRange(sector[..take]);
            }
        }

        return result.ToArray();
    }
}
