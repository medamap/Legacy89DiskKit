using System.Buffers.Binary;
using System.Text;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosDirParser
{
    private const int EntrySize = 32;
    private const int FirstDirR = 2;
    private const int LastDirR  = 10;

    public IReadOnlyList<XDosDirectoryEntry> Parse(IDiskContainer container)
    {
        var entries = new List<XDosDirectoryEntry>();
        for (int r = FirstDirR; r <= LastDirR; r++)
        {
            var sector = container.ReadSector(1, 0, r);
            for (int offset = 0; offset + EntrySize <= sector.Length; offset += EntrySize)
            {
                var entry = ParseEntry(sector, offset);
                if (!entry.IsEmpty)
                    entries.Add(entry);
            }
        }
        return entries;
    }

    private static XDosDirectoryEntry ParseEntry(byte[] sector, int offset)
    {
        byte rawType = sector[offset + 0];
        byte attr    = sector[offset + 1];
        var rawName  = sector[(offset + 2)..(offset + 18)];
        string name  = Encoding.ASCII.GetString(rawName).TrimEnd(' ');
        ushort load  = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 20));
        ushort end   = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 22));
        ushort exec  = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 24));
        byte flags   = sector[offset + 28];
        byte cluster = sector[offset + 29];
        byte startR  = sector[offset + 30];
        byte always1 = sector[offset + 31];

        return new XDosDirectoryEntry(rawType, attr, name, rawName,
            load, end, exec, flags, cluster, startR, always1);
    }
}
