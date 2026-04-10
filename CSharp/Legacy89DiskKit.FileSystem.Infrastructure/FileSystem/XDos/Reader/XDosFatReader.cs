using System.Buffers.Binary;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosFatReader
{
    private const int BitmapOffset = 0xA8;

    private readonly byte[]           _fatSector;
    private readonly XDosMediaGeometry _geometry;

    public XDosFatReader(IDiskContainer container, XDosMediaGeometry geometry)
    {
        _fatSector = container.ReadSector(0, 1, 1);
        _geometry  = geometry;
    }

    public byte[] FatSector => _fatSector;

    public bool IsSectorFree(int track, int sector)
    {
        int offset = BitmapOffset + track * 2;
        if (offset + 1 >= _fatSector.Length) return false;
        ushort word = BinaryPrimitives.ReadUInt16BigEndian(_fatSector.AsSpan(offset));
        return ((word >> (16 - sector)) & 1) == 1;
    }

    public int CountFreeRecords()
    {
        int count = 0;
        for (int t = 2; t < _geometry.TotalTracks; t++)
            for (int s = 1; s <= _geometry.DataSectorsPerTrack; s++)
                if (IsSectorFree(t, s)) count++;
        return count;
    }

    public int CountUsedRecords()
    {
        int count = 0;
        for (int t = 2; t < _geometry.TotalTracks; t++)
            for (int s = 1; s <= _geometry.DataSectorsPerTrack; s++)
                if (!IsSectorFree(t, s)) count++;
        return count;
    }
}
