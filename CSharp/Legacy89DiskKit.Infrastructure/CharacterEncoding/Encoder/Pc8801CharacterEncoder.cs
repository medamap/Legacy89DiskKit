using System.Text;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;

public class Pc8801CharacterEncoder : ICharacterEncoder
{
    public string EncodingId => "PC88";
    public MachineType SupportedMachine => MachineType.PC8801;

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
