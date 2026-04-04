using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.FileSystem.Infrastructure.HuBasic.Models;

namespace Legacy89DiskKit.FileSystem.Infrastructure.HuBasic;

public static class HuBasicDirectoryEntryCodec
{
    public static HuBasicDirectoryEntryData Parse(byte[] data, ICharacterEncoder encoder)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (encoder == null) throw new ArgumentNullException(nameof(encoder));
        if (data.Length < 32) throw new ArgumentException("Hu-BASIC directory entries must be 32 bytes long.", nameof(data));

        byte modeByte = data[0];
        byte passwordByte = data[0x11];
        var nameBytes = data.Skip(1).Take(13).ToArray();
        var extBytes = data.Skip(0x0E).Take(3).ToArray();
        string fileName = encoder.DecodeText(nameBytes).TrimEnd(' ');
        string extension = encoder.DecodeText(extBytes).TrimEnd(' ');
        ushort size = BitConverter.ToUInt16(data, 0x12);
        ushort loadAddress = BitConverter.ToUInt16(data, 0x14);
        ushort executionAddress = BitConverter.ToUInt16(data, 0x16);
        DateTime modifiedDate = ParseBcdDate(data, 0x18);
        int startCluster = (data[0x1F] << 7) | (data[0x1E] & 0x7F);

        return new HuBasicDirectoryEntryData(
            modeByte,
            passwordByte,
            nameBytes,
            extBytes,
            fileName,
            extension,
            size,
            loadAddress,
            executionAddress,
            modifiedDate,
            startCluster);
    }

    public static void WriteToBuffer(byte[] buffer, int offset, HuBasicDirectoryEntryData entry, ICharacterEncoder encoder)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (encoder == null) throw new ArgumentNullException(nameof(encoder));
        if (offset < 0 || offset + 32 > buffer.Length) throw new ArgumentOutOfRangeException(nameof(offset), "A Hu-BASIC directory entry requires 32 writable bytes.");

        buffer[offset] = entry.ModeByte;

        var nameBytes = entry.RawFileName.Length == 13
            ? entry.RawFileName
            : encoder.EncodeText(entry.FileName.PadRight(13));
        for (int i = 0; i < 13; i++) buffer[offset + 1 + i] = i < nameBytes.Length ? nameBytes[i] : (byte)0x20;

        var extBytes = entry.RawExtension.Length == 3
            ? entry.RawExtension
            : encoder.EncodeText(entry.Extension.PadRight(3));
        for (int i = 0; i < 3; i++) buffer[offset + 0x0E + i] = i < extBytes.Length ? extBytes[i] : (byte)0x20;

        buffer[offset + 0x11] = entry.PasswordByte;
        BitConverter.GetBytes(entry.RecordedSize).CopyTo(buffer, offset + 0x12);
        BitConverter.GetBytes(entry.LoadAddress).CopyTo(buffer, offset + 0x14);
        BitConverter.GetBytes(entry.ExecutionAddress).CopyTo(buffer, offset + 0x16);
        WriteBcdDate(buffer, offset + 0x18, entry.ModifiedDate);
        buffer[offset + 0x1D] = (byte)((entry.StartCluster >> 14) & 0x7F);
        buffer[offset + 0x1E] = (byte)(entry.StartCluster & 0x7F);
        buffer[offset + 0x1F] = (byte)((entry.StartCluster >> 7) & 0x7F);
    }

    private static DateTime ParseBcdDate(byte[] data, int offset)
    {
        try
        {
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
        }
        catch
        {
            return DateTime.MinValue;
        }
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
