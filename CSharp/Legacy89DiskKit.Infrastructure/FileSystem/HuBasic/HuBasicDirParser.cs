using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.Models;
using System.Text;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public class HuBasicDirParser
{
    private readonly HuBasicConfiguration _config;

    private readonly Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder.X1CharacterEncoder _encoder = new();

    public HuBasicDirParser(HuBasicConfiguration config)
    {
        _config = config;
    }

    public FileEntry Parse(byte[] data)
    {
        byte modeByte = data[0];
        
        // Decode filename and extension using X1 encoder
        var nameBytes = data.Skip(1).Take(13).ToArray();
        var extBytes = data.Skip(0x0E).Take(3).ToArray();
        
        string fileName = _encoder.DecodeText(nameBytes).TrimEnd(' ');
        string extension = _encoder.DecodeText(extBytes).TrimEnd(' ');
        
        ushort size = BitConverter.ToUInt16(data, 0x12);
        ushort loadAddr = BitConverter.ToUInt16(data, 0x14);
        ushort execAddr = BitConverter.ToUInt16(data, 0x16);
        DateTime modifiedDate = ParseBcdDate(data, 0x18);
        
        // Start Cluster: 0x1D: HIGH, 0x1E: LOW, 0x1F: MIDDLE (7 bits each)
        int startCluster = (data[0x1F] << 7) | (data[0x1E] & 0x7F);
        // Note: HIGH (0x1D) is also 7 bits but for 2D/2DD/2HD it's usually 0 since max cluster is 250.
        // If we needed it: startCluster |= (data[0x1D] & 0x7F) << 14;

        var standardAttr = Domain.FileSystem.Model.FileAttributes.None;
        if ((modeByte & 0x80) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.Directory;
        if ((modeByte & 0x40) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.ReadOnly;
        if ((modeByte & 0x20) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.System;
        if ((modeByte & 0x10) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.Hidden;

        bool isAscii = (modeByte & 0x04) != 0 || (modeByte & 0x08) != 0;

        var extAttr = new ExtendedFileAttributes(
            standardAttr,
            modeByte,
            isAscii,
            $"Raw:0x{modeByte:X2}"
        );

        return new FileEntry(
            fileName,
            extension,
            size,
            null, // CreatedAt not supported
            modifiedDate,
            extAttr,
            startCluster,
            loadAddr,
            (ushort?)(loadAddr + size - 1),
            execAddr
        );
    }

    public void WriteToBuffer(byte[] buffer, int offset, FileEntry entry)
    {
        var att = entry.Attributes;
        byte modeByte = att.RawAttributes;
        
        // Reset and set flags based on standard attributes
        modeByte &= 0x0F; // Preserve lower bits (ASCII/BIN)
        if ((att.StandardAttributes & Domain.FileSystem.Model.FileAttributes.Directory) != 0) modeByte |= 0x80;
        if ((att.StandardAttributes & Domain.FileSystem.Model.FileAttributes.ReadOnly) != 0) modeByte |= 0x40;
        if ((att.StandardAttributes & Domain.FileSystem.Model.FileAttributes.System) != 0) modeByte |= 0x20;
        if ((att.StandardAttributes & Domain.FileSystem.Model.FileAttributes.Hidden) != 0) modeByte |= 0x10;

        buffer[offset] = modeByte;
        
        var nameBytes = _encoder.EncodeText(entry.FileName.PadRight(13));
        for (int i = 0; i < 13; i++) buffer[offset + 1 + i] = i < nameBytes.Length ? nameBytes[i] : (byte)0x20;
        
        var extBytes = _encoder.EncodeText(entry.Extension.PadRight(3));
        for (int i = 0; i < 3; i++) buffer[offset + 0x0E + i] = i < extBytes.Length ? extBytes[i] : (byte)0x20;
        
        buffer[offset + 0x11] = 0x20; // Password block defaults to space (no password)
        
        BitConverter.GetBytes((ushort)entry.Size).CopyTo(buffer, offset + 0x12);
        
        ushort loadAddr = entry.LoadAddress ?? 0;
        ushort execAddr = entry.ExecutionAddress ?? 0;

        BitConverter.GetBytes(loadAddr).CopyTo(buffer, offset + 0x14);
        BitConverter.GetBytes(execAddr).CopyTo(buffer, offset + 0x16);
        
        WriteBcdDate(buffer, offset + 0x18, entry.LastModifiedAt ?? DateTime.Now);
        
        buffer[offset + 0x1D] = (byte)((entry.StartCluster >> 14) & 0x7F);
        buffer[offset + 0x1E] = (byte)(entry.StartCluster & 0x7F);
        buffer[offset + 0x1F] = (byte)((entry.StartCluster >> 7) & 0x7F);
    }

    private static DateTime ParseBcdDate(byte[] data, int offset)
    {
        try {
            int year = BcdToByte(data[offset]);
            int monthDay = data[offset + 1];
            int month = (monthDay >> 4) & 0x0F;
            int day = BcdToByte(data[offset + 2]);
            int hour = BcdToByte(data[offset + 3]);
            int minute = BcdToByte(data[offset + 4]);
            
            int fullYear = year < 80 ? 2000 + year : 1900 + year;
            if (month < 1 || month > 12) month = 1;
            if (day < 1 || day > 31) day = 1;
            
            return new DateTime(fullYear, month, day, hour % 24, minute % 60, 0);
        } catch { return DateTime.MinValue; }
    }

    private static void WriteBcdDate(byte[] data, int offset, DateTime date)
    {
        data[offset] = ByteToBcd(date.Year % 100);
        data[offset + 1] = (byte)((date.Month << 4) | (int)date.DayOfWeek);
        data[offset + 2] = ByteToBcd(date.Day);
        data[offset + 3] = ByteToBcd(date.Hour);
        data[offset + 4] = ByteToBcd(date.Minute);
    }

    private static byte BcdToByte(byte bcd) => (byte)((bcd >> 4) * 10 + (bcd & 0x0F));
    private static byte ByteToBcd(int value) => (byte)(((value / 10) << 4) | (value % 10));
}
