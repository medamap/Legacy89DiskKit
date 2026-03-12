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
        byte passwordByte = data[0x11];
        
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

        var standardAttr = Domain.FileSystem.Model.FileAttributes.None;
        if ((modeByte & 0x80) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.Directory;
        if ((modeByte & 0x40) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.ReadOnly;
        if ((modeByte & 0x10) != 0) standardAttr |= Domain.FileSystem.Model.FileAttributes.Hidden;

        var fileType = GetFileType(modeByte);
        bool isAscii = fileType == HuBasicFileType.Ascii;

        var extAttr = new ExtendedFileAttributes(
            standardAttr,
            modeByte,
            isAscii,
            "Hu-BASIC"
        );

        var metadata = new HuBasicFileMetadata(
            fileType,
            passwordByte != 0x20,
            (modeByte & 0x10) != 0,
            (modeByte & 0x20) != 0,
            (modeByte & 0x40) != 0,
            (modeByte & 0x80) != 0,
            size,
            loadAddr,
            execAddr,
            startCluster,
            modeByte,
            passwordByte
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
            execAddr,
            nameBytes,
            extBytes,
            metadata
        );
    }

    public void WriteToBuffer(byte[] buffer, int offset, FileEntry entry)
    {
        var att = entry.Attributes;
        var metadata = entry.FileSystemMetadata as HuBasicFileMetadata;
        byte modeByte = metadata != null
            ? BuildModeByte(metadata)
            : BuildModeByte(att, entry);

        buffer[offset] = modeByte;
        
        // Use RawFileName if available and of correct length, otherwise encode string
        var nameBytes = (entry.RawFileName != null && entry.RawFileName.Length == 13) 
            ? entry.RawFileName 
            : _encoder.EncodeText(entry.FileName.PadRight(13));

        for (int i = 0; i < 13; i++) buffer[offset + 1 + i] = i < nameBytes.Length ? nameBytes[i] : (byte)0x20;
        
        var extBytes = (entry.RawExtension != null && entry.RawExtension.Length == 3)
            ? entry.RawExtension
            : _encoder.EncodeText(entry.Extension.PadRight(3));

        for (int i = 0; i < 3; i++) buffer[offset + 0x0E + i] = i < extBytes.Length ? extBytes[i] : (byte)0x20;
        
        buffer[offset + 0x11] = metadata?.PasswordByte ?? 0x20;
        
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

    private static HuBasicFileType GetFileType(byte modeByte)
    {
        if ((modeByte & 0x01) != 0) return HuBasicFileType.Binary;
        if ((modeByte & 0x02) != 0) return HuBasicFileType.Basic;
        if ((modeByte & 0x0C) != 0) return HuBasicFileType.Ascii;
        return HuBasicFileType.Unknown;
    }

    private static byte BuildModeByte(HuBasicFileMetadata metadata)
    {
        byte modeByte = 0;

        if (metadata.IsDirectory) modeByte |= 0x80;
        if (metadata.IsWriteProtected) modeByte |= 0x40;
        if (metadata.IsVerify) modeByte |= 0x20;
        if (metadata.IsHidden) modeByte |= 0x10;

        modeByte |= metadata.FileType switch
        {
            HuBasicFileType.Binary => (byte)0x01,
            HuBasicFileType.Basic => (byte)0x02,
            HuBasicFileType.Ascii => (byte)0x04,
            _ => (byte)0x00
        };

        return modeByte;
    }

    private static byte BuildModeByte(ExtendedFileAttributes attributes, FileEntry entry)
    {
        byte modeByte = (byte)(attributes.RawAttributes & 0x0F);

        if ((attributes.StandardAttributes & Domain.FileSystem.Model.FileAttributes.Directory) != 0) modeByte |= 0x80;
        if ((attributes.StandardAttributes & Domain.FileSystem.Model.FileAttributes.ReadOnly) != 0) modeByte |= 0x40;
        if ((attributes.StandardAttributes & Domain.FileSystem.Model.FileAttributes.Hidden) != 0) modeByte |= 0x10;

        if ((modeByte & 0x0F) == 0)
        {
            modeByte |= entry.Attributes.IsAscii ? (byte)0x04 : (byte)0x01;
        }

        return modeByte;
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
