using System.Buffers.Binary;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosDirWriter
{
    private const int EntrySize   = 32;
    private const int DirCylinder = 0;
    private const int DirHead     = 1;
    private const int FirstDirR   = 2;

    private readonly IDiskContainer _container;
    private readonly int            _lastDirR;

    public XDosDirWriter(IDiskContainer container)
    {
        _container = container;
        _lastDirR  = container.DiskType == DiskType.TwoHD ? 16 : 10;
    }

    public void WriteEntry(XDosDirectoryEntry entry)
    {
        var (r, offset) = FindFreeSlot();
        var sector = _container.ReadSector(DirCylinder, DirHead, r);
        SerializeEntry(entry, sector, offset);
        _container.WriteSector(DirCylinder, DirHead, r, sector);
    }

    public void WriteEntry(XDosDirectoryEntry entry, int sectorNumber, int offset)
    {
        if (sectorNumber < FirstDirR || sectorNumber > _lastDirR)
            throw new IOException("Directory slot out of range.");
        if (!_container.SectorExists(DirCylinder, DirHead, sectorNumber))
            throw new IOException("Directory sector not found.");

        var sector = _container.ReadSector(DirCylinder, DirHead, sectorNumber);
        if (offset < 0 || offset + EntrySize > sector.Length || offset % EntrySize != 0)
            throw new IOException("Directory offset out of range.");

        ushort rawType = BinaryPrimitives.ReadUInt16BigEndian(sector.AsSpan(offset));
        if (rawType != 0x0000 && rawType != 0xFFFF)
            throw new IOException("Directory slot already in use.");

        SerializeEntry(entry, sector, offset);
        _container.WriteSector(DirCylinder, DirHead, sectorNumber, sector);
    }

    private (int r, int offset) FindFreeSlot()
    {
        for (int r = FirstDirR; r <= _lastDirR; r++)
        {
            if (!_container.SectorExists(DirCylinder, DirHead, r)) continue;
            var sector = _container.ReadSector(DirCylinder, DirHead, r);
            for (int offset = 0; offset + EntrySize <= sector.Length; offset += EntrySize)
            {
                ushort rawType = BinaryPrimitives.ReadUInt16BigEndian(sector.AsSpan(offset));
                if (rawType == 0x0000 || rawType == 0xFFFF) return (r, offset);
            }
        }
        throw new IOException("Directory full.");
    }

    private static void SerializeEntry(XDosDirectoryEntry entry, byte[] buffer, int offset)
    {
        BinaryPrimitives.WriteUInt16BigEndian   (buffer.AsSpan(offset + 0x00), entry.RawFileType);
        Array.Copy(entry.RawFileName, 0, buffer, offset + 0x02, Math.Min(entry.RawFileName.Length, 16));
        for (int i = entry.RawFileName.Length; i < 16; i++) buffer[offset + 0x02 + i] = 0x20;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset + 0x12), entry.StartAddress);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset + 0x14), entry.SizeLow);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset + 0x16), entry.ExecAddressOrSizeHigh);
        BinaryPrimitives.WriteUInt32BigEndian   (buffer.AsSpan(offset + 0x18), entry.TimestampRaw);
        buffer[offset + 0x1C] = entry.Attribute;
        buffer[offset + 0x1D] = entry.FamPointer.Track;
        buffer[offset + 0x1E] = entry.FamPointer.Sector;
        buffer[offset + 0x1F] = entry.FamPointer.Record;
    }

    public void ClearAll()
    {
        var empty = new byte[512];
        for (int r = FirstDirR; r <= _lastDirR; r++)
            _container.WriteSector(DirCylinder, DirHead, r, empty);
    }
}
