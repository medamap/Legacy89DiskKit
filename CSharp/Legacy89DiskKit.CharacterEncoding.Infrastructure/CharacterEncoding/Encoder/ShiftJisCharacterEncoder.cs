using System.Text;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

public class ShiftJisCharacterEncoder : ICharacterEncoder
{
    private readonly Encoding _sjis;
    public string EncodingId => "SJIS";

    public ShiftJisCharacterEncoder()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _sjis = Encoding.GetEncoding(932); // Shift-JIS (Japanese)
    }

    public MachineType SupportedMachine => MachineType.Unknown; // Generic

    public byte[] EncodeText(string text)
    {
        return _sjis.GetBytes(text);
    }

    public string DecodeText(byte[] data)
    {
        return DecodeText(data, Environment.NewLine);
    }

    public string DecodeText(byte[] data, string newline)
    {
        string decoded = _sjis.GetString(data);
        // Normalize newlines if necessary
        return decoded.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", newline);
    }
}
