using System.Text;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;

public class ShiftJisCharacterEncoder : ICharacterEncoder
{
    private readonly Encoding _sjis;

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
