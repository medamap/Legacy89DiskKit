using System.Text;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.X1;

namespace Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;

public class X1CharacterEncoder : ICharacterEncoder
{
    public string EncodingId => "X1";
    public MachineType SupportedMachine => MachineType.X1;

    public string DecodeText(byte[] data)
    {
        return DecodeText(data, Environment.NewLine);
    }

    public string DecodeText(byte[] data, string newline)
    {
        var sb = new StringBuilder();
        foreach (var b in data)
        {
            if (b == 0x1A) break; // EOF
            
            if (b == 0x0D)
            {
                sb.Append(newline);
            }
            else
            {
                sb.Append(X1CharacterMap.ByteToText[b]);
            }
        }
        return sb.ToString();
    }

    public byte[] EncodeText(string text)
    {
        // 1. Normalize Newlines to CR (0x0D)
        string normalized = text.Replace("\r\n", "\r").Replace("\n", "\r");
        
        var result = new List<byte>();
        foreach (var ch in normalized)
        {
            string s = ch.ToString();
            if (X1CharacterMap.TextToByte.TryGetValue(s, out var b))
            {
                result.Add(b);
            }
            else
            {
                // Fallback for full-width characters if necessary
                // Check if it's a known mapping or just space
                result.Add(0x20); // Space
            }
        }

        return result.ToArray();
    }
}
