using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;

namespace Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

public class HuBasicBootRecordParser
{
    private readonly X1CharacterEncoder _encoder = new();

    public HuBasicBootRecordInfo? Parse(byte[] bootArea)
    {
        if (bootArea.Length < 32)
        {
            return null;
        }

        byte bootFlag = bootArea[0];
        if (bootFlag == 0x00)
        {
            return null;
        }

        string name = _encoder.DecodeText(bootArea.Skip(1).Take(13).ToArray()).TrimEnd(' ');
        string extension = _encoder.DecodeText(bootArea.Skip(0x0E).Take(3).ToArray()).TrimEnd(' ');
        byte passwordByte = bootArea[0x11];
        ushort size = BitConverter.ToUInt16(bootArea, 0x12);
        ushort loadAddress = BitConverter.ToUInt16(bootArea, 0x14);
        ushort executionAddress = BitConverter.ToUInt16(bootArea, 0x16);
        ushort startRecord = BitConverter.ToUInt16(bootArea, 0x1E);

        return new HuBasicBootRecordInfo(
            bootFlag,
            name,
            extension,
            passwordByte != 0x20,
            size,
            loadAddress,
            executionAddress,
            startRecord
        );
    }
}
