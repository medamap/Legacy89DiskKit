using System.Buffers.Binary;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosDirWriter
{
    private const int EntrySize = 32;
    private const int DirCylinder = 0;
    private const int DirHead = 1;
    private const int FirstDirR = 2;
    private const int LastDirR = 10;

    private readonly IDiskContainer _container;
    public XDosDirWriter(IDiskContainer container) => _container = container;

    public void WriteEntry(XDosDirectoryEntry entry)
    {
        var (r, offset) = FindFreeSlot();
        var sector = _container.ReadSector(DirCylinder, DirHead, r);
        SerializeEntry(entry, sector, offset);
        _container.WriteSector(DirCylinder, DirHead, r, sector);
    }

    private (int r, int offset) FindFreeSlot()
    {
        for (int r = FirstDirR; r <= LastDirR; r++)
        {
            if (!_container.SectorExists(DirCylinder, DirHead, r)) continue;
            var sector = _container.ReadSector(DirCylinder, DirHead, r);
            for (int offset = 0; offset + EntrySize <= sector.Length; offset += EntrySize)
            {
                if (sector[offset] == 0x00 || sector[offset] == 0xFF || sector[offset] == 0xD5) return (r, offset);
            }
        }
        throw new Exception("Directory full.");
    }

    private static void SerializeEntry(XDosDirectoryEntry entry, byte[] buffer, int offset)
    {
        buffer[offset + 0] = entry.RawFileType;
        buffer[offset + 1] = entry.Attribute;
        Array.Copy(entry.RawFileName, 0, buffer, offset + 2, Math.Min(entry.RawFileName.Length, 16));
        for (int i = entry.RawFileName.Length; i < 16; i++) buffer[offset + 2 + i] = 0x20;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset + 20), entry.LoadAddress);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset + 22), entry.EndAddress);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset + 24), entry.ExecutionAddress);
        buffer[offset + 28] = entry.Flags;
        buffer[offset + 29] = entry.FirstCluster;
        buffer[offset + 30] = entry.FirstSectorR;
        buffer[offset + 31] = entry.AlwaysOne;
    }

    public void ClearAll()
    {
        var empty = new byte[512];
        for (int r = FirstDirR; r <= LastDirR; r++) _container.WriteSector(DirCylinder, DirHead, r, empty);
    }
}
