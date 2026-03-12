using System.Text;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;

public class Msx1CharacterEncoder : ICharacterEncoder
{
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
