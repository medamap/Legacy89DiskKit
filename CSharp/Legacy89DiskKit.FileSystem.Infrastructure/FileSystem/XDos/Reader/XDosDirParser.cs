using System.Buffers.Binary;
using System.Text;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.DiskImage.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Model.XDos;

namespace Legacy89DiskKit.FileSystem.Infrastructure.XDos.Reader;

public class XDosDirParser
{
    private const int EntrySize = 32;

    public IReadOnlyList<XDosDirectoryEntry> Parse(IDiskContainer container)
    {
        var entries   = new List<XDosDirectoryEntry>();
        int cylinder  = 0;
        int head      = 1;
        int maxSector = container.DiskType == DiskType.TwoHD ? 16 : 10;
        for (int r = 2; r <= maxSector; r++)
        {
            if (!container.SectorExists(cylinder, head, r)) continue;
            var sector = container.ReadSector(cylinder, head, r);
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
        ushort rawType               = BinaryPrimitives.ReadUInt16BigEndian(sector.AsSpan(offset + 0x00));
        var    rawName               = sector[(offset + 0x02)..(offset + 0x12)];
        string name                  = Encoding.Latin1.GetString(rawName).TrimEnd(' ');
        ushort startAddress          = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 0x12));
        ushort sizeLow               = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 0x14));
        ushort execAddressOrSizeHigh = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 0x16));
        uint   timestampRaw          = BinaryPrimitives.ReadUInt32BigEndian(sector.AsSpan(offset + 0x18));
        byte   attribute             = sector[offset + 0x1C];
        var    famPointer            = new XDosFamPointer(sector[offset + 0x1D], sector[offset + 0x1E], sector[offset + 0x1F]);
        return new XDosDirectoryEntry(rawType, name, rawName, startAddress, sizeLow, execAddressOrSizeHigh, timestampRaw, attribute, famPointer);
    }
}
