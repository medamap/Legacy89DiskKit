using System;

namespace Legacy89DiskKit.FileSystem.Infrastructure.XDos;

/// <summary>
/// Helper for decoding X-DOS raw BCD timestamp into a .NET DateTime.
/// </summary>
public static class XDosTimestampHelper
{
    /// <summary>
    /// Decodes a 4-byte X-DOS raw timestamp (BE: 0xYY 0xMM 0xDD 0x??) into a DateTime.
    /// Year is offset from 2000. Month and Day are BCD encoded.
    /// If raw value is 0 or invalid BCD, returns null.
    /// </summary>
    public static DateTime? DecodeTimestamp(uint raw)
    {
        if (raw == 0) return null;

        // X-DOS format: 0xYY 0xMM 0xDD 0x?? (read as Big-Endian uint)
        // or 3 bytes: 0xYY 0xMM 0xDD
        byte yRaw = (byte)(raw >> 16);
        byte mRaw = (byte)(raw >> 8);
        byte dRaw = (byte)(raw);

        int y = ((yRaw >> 4) & 0xF) * 10 + (yRaw & 0xF) + 2000;
        int m = ((mRaw >> 4) & 0xF) * 10 + (mRaw & 0xF);
        int d = ((dRaw >> 4) & 0xF) * 10 + (dRaw & 0xF);

        if (m < 1 || m > 12 || d < 1 || d > 31) return null;

        try
        {
            return new DateTime(y, m, d, 0, 0, 0, DateTimeKind.Unspecified);
        }
        catch
        {
            return null;
        }
    }
}
