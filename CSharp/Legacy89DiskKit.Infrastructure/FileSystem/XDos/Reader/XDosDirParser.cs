using System.Buffers.Binary;
using System.Text;
using Legacy89DiskKit.Domain.DiskImage.Interface.Container;
using Legacy89DiskKit.Domain.FileSystem.Model.XDos;

namespace Legacy89DiskKit.Infrastructure.FileSystem.XDos.Reader;

public class XDosDirParser
{
    private const int EntrySize = 32;
    public IReadOnlyList<XDosDirectoryEntry> Parse(IDiskContainer container)
    {
        var entries = new List<XDosDirectoryEntry>();
        // Directory resides on Track 1 (C0, H1, R2-10)
        int cylinder = 0;
        int head     = 1;
        for (int r = 2; r <= 10; r++)
        {
            if (!container.SectorExists(cylinder, head, r)) continue;
            var sector = container.ReadSector(cylinder, head, r);
            for (int offset = 0; offset + EntrySize <= sector.Length; offset += EntrySize)
            {
                var entry = ParseEntry(sector, offset);
                // 変更後:（IsEmpty でない全エントリを有効とする）
                if (!entry.IsEmpty)
                {
                    entries.Add(entry);
                }
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
        ushort load  = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 18)); // 修正: +20 → +18
        ushort size  = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 20)); // 修正: +22 → +20
        ushort exec  = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 22)); // 修正: +24 → +22
        // 24-25: packed date (big-endian on disk)
        ushort date  = BinaryPrimitives.ReadUInt16BigEndian(sector.AsSpan(offset + 24));
        // 26-27: packed time (big-endian on disk)
        ushort time  = BinaryPrimitives.ReadUInt16BigEndian(sector.AsSpan(offset + 26));
        byte flags   = sector[offset + 28];
        byte cluster = sector[offset + 29];
        byte startR  = sector[offset + 30];
        byte always1 = sector[offset + 31];
        return new XDosDirectoryEntry(rawType, attr, name, rawName, load, size, exec, date, time, flags, cluster, startR, always1);
    }
}
