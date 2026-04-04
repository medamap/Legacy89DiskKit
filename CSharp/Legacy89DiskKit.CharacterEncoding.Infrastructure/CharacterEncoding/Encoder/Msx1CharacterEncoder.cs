using System.Text;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

public class Msx1CharacterEncoder : ICharacterEncoder
{
    public string EncodingId => "MSX";
    public MachineType SupportedMachine => MachineType.MSX;

    public string DecodeText(byte[] data) => DecodeText(data, Environment.NewLine);

    public string DecodeText(byte[] data, string newline)
    {
        // Placeholder
        return Encoding.ASCII.GetString(data);
    }

    public byte[] EncodeText(string text)
    {
        // Placeholder: Use Shift-JIS or ASCII
        return Encoding.ASCII.GetBytes(text);
    }
}
