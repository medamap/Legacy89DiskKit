using System.Buffers.Binary;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFatWriter
{
    private const int BitmapOffset = 0xA8;

    private readonly IDiskContainer    _container;
    private readonly XDosMediaGeometry _geometry;
    private readonly byte[]            _fatSector;

    public XDosFatWriter(IDiskContainer container, XDosMediaGeometry geometry)
    {
        _container = container;
        _geometry  = geometry;
        _fatSector = container.ReadSector(0, 1, 1);
    }

    public byte[] FatSector => _fatSector;

    public bool IsSectorFree(int track, int sector)
    {
        int offset = BitmapOffset + track * 2;
        if (offset + 1 >= _fatSector.Length) return false;
        ushort word = BinaryPrimitives.ReadUInt16BigEndian(_fatSector.AsSpan(offset));
        return ((word >> (16 - sector)) & 1) == 1;
    }

    private void SetSectorUsed(int track, int sector)
    {
        int offset = BitmapOffset + track * 2;
        ushort word = BinaryPrimitives.ReadUInt16BigEndian(_fatSector.AsSpan(offset));
        word = (ushort)(word & ~(1 << (16 - sector)));
        BinaryPrimitives.WriteUInt16BigEndian(_fatSector.AsSpan(offset), word);
    }

    public void MarkUsed(int track, int sector) => SetSectorUsed(track, sector);

    private void SetSectorFree(int track, int sector)
    {
        int offset = BitmapOffset + track * 2;
        ushort word = BinaryPrimitives.ReadUInt16BigEndian(_fatSector.AsSpan(offset));
        word = (ushort)(word | (1 << (16 - sector)));
        BinaryPrimitives.WriteUInt16BigEndian(_fatSector.AsSpan(offset), word);
    }

    public void MarkFree(int track, int sector) => SetSectorFree(track, sector);

    public List<(int Track, int Sector)> AllocateRecords(int count)
    {
        var allocated = new List<(int, int)>();
        for (int t = 2; t < _geometry.TotalTracks && allocated.Count < count; t++)
            for (int s = 1; s <= _geometry.DataSectorsPerTrack && allocated.Count < count; s++)
                if (IsSectorFree(t, s))
                {
                    SetSectorUsed(t, s);
                    allocated.Add((t, s));
                }
        if (allocated.Count < count) throw new IOException("Disk full.");
        return allocated;
    }

    public void Commit() => _container.WriteSector(0, 1, 1, _fatSector);

    public void ClearAll()
    {
        Array.Clear(_fatSector, 0, _fatSector.Length);
        _fatSector[0x00] = _geometry.DataSectorsPerTrack == 16 ? (byte)0x02 :
                           _geometry.TotalTracks > 80           ? (byte)0x01 : (byte)0x00;
        _fatSector[0x01] = 0x01;
        ushort freePattern = _geometry.DataSectorsPerTrack == 16 ? (ushort)0xFFFF : (ushort)0xFFC0;
        for (int t = 2; t < _geometry.TotalTracks; t++)
        {
            int off = BitmapOffset + t * 2;
            if (off + 1 < _fatSector.Length)
                BinaryPrimitives.WriteUInt16BigEndian(_fatSector.AsSpan(off), freePattern);
        }
    }
}
