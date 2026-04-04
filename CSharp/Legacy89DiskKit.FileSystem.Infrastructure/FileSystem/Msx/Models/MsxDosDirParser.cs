using System.Text;
using Legacy89DiskKit.FileSystem.Domain.Model;
using DomainAttr = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Msx.Models;

public class MsxDosDirParser
{
    public FileEntry Parse(byte[] data)
    {
        string name = Encoding.ASCII.GetString(data, 0, 8).TrimEnd();
        string ext = Encoding.ASCII.GetString(data, 8, 3).TrimEnd();
        byte attrByte = data[11];
        ushort startCluster = BitConverter.ToUInt16(data, 26);
        uint size = BitConverter.ToUInt32(data, 28);

        var attr = DomainAttr.None;
        if ((attrByte & 0x01) != 0) attr |= DomainAttr.ReadOnly;
        if ((attrByte & 0x02) != 0) attr |= DomainAttr.Hidden;
        if ((attrByte & 0x04) != 0) attr |= DomainAttr.System;
        if ((attrByte & 0x10) != 0) attr |= DomainAttr.Directory;
        if ((attrByte & 0x20) != 0) attr |= DomainAttr.Archive;

        var extendedAttr = new ExtendedFileAttributes(
            attr,
            attrByte,
            (attrByte & 0x10) == 0, // Assume ASCII if not a directory for MSX-DOS
            "MSX"
        );

        // Date/Time parsing
        ushort timeRaw = BitConverter.ToUInt16(data, 22);
        ushort dateRaw = BitConverter.ToUInt16(data, 24);
        DateTime dateTime = ParseFatDateTime(dateRaw, timeRaw);

        var nameBytes = data.Take(8).ToArray();
        var extBytes = data.Skip(8).Take(3).ToArray();

        return new FileEntry(name, ext, size, dateTime, dateTime, extendedAttr, startCluster, null, null, null, nameBytes, extBytes);
    }

    public void WriteToBuffer(byte[] buffer, int offset, FileEntry entry)
    {
        Array.Clear(buffer, offset, 32);
        byte[] nameBytes = Encoding.ASCII.GetBytes(entry.FileName.PadRight(8).Substring(0, 8));
        Array.Copy(nameBytes, 0, buffer, offset, 8);
        byte[] extBytes = Encoding.ASCII.GetBytes(entry.Extension.PadRight(3).Substring(0, 3));
        Array.Copy(extBytes, 0, buffer, offset + 8, 3);
        
        buffer[offset + 11] = entry.Attributes.RawAttributes;
        
        // Date/Time
        var dt = entry.LastModifiedAt ?? entry.CreatedAt ?? DateTime.Now;
        var (date, time) = SerializeFatDateTime(dt);
        byte[] timeBytes = BitConverter.GetBytes(time);
        byte[] dateBytes = BitConverter.GetBytes(date);
        Array.Copy(timeBytes, 0, buffer, offset + 22, 2);
        Array.Copy(dateBytes, 0, buffer, offset + 24, 2);

        byte[] clusterBytes = BitConverter.GetBytes((ushort)entry.StartCluster);
        Array.Copy(clusterBytes, 0, buffer, offset + 26, 2);
        
        byte[] sizeBytes = BitConverter.GetBytes((uint)entry.Size);
        Array.Copy(sizeBytes, 0, buffer, offset + 28, 4);
    }

    private DateTime ParseFatDateTime(ushort date, ushort time)
    {
        try
        {
            int year = ((date >> 9) & 0x7F) + 1980;
            int month = (date >> 5) & 0x0F;
            int day = date & 0x1F;
            int hour = (time >> 11) & 0x1F;
            int minute = (time >> 5) & 0x3F;
            int second = (time & 0x1F) * 2;
            
            if (month < 1 || month > 12 || day < 1 || day > 31) return DateTime.MinValue;
            return new DateTime(year, month, day, hour, minute, Math.Min(second, 59));
        }
        catch { return DateTime.MinValue; }
    }

    private (ushort date, ushort time) SerializeFatDateTime(DateTime dt)
    {
        ushort date = (ushort)(((dt.Year - 1980) << 9) | (dt.Month << 5) | dt.Day);
        ushort time = (ushort)((dt.Hour << 11) | (dt.Minute << 5) | (dt.Second / 2));
        return (date, time);
    }
}
