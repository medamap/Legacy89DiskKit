using System.Text;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

public class Msx1CharacterEncoder : ICharacterEncoder
{
    public MachineType SupportedMachine => MachineType.Msx1;

    public string DecodeText(byte[] data)
    {
        // TODO: Implement MSX1 specific character encoding
        // For now, use basic ASCII mapping with fallback to '□' for unmapped characters
        var result = new StringBuilder();
        foreach (var b in data)
        {
            if (b >= 0x20 && b <= 0x7E)
                result.Append((char)b);
            else
                result.Append('□'); // fallback for unmapped characters
        }
        return result.ToString();
    }

    public byte[] EncodeText(string text)
    {
        // TODO: Implement MSX1 specific character encoding
        // For now, use basic ASCII encoding with fallback to space
        var result = new List<byte>();
        foreach (var ch in text)
        {
            if (ch >= 0x20 && ch <= 0x7E)
                result.Add((byte)ch);
            else
                result.Add(0x20); // fallback to space
        }
        return result.ToArray();
    }
}