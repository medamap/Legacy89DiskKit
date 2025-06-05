using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using System.Text;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

/// <summary>
/// Character encoder for generic CP/M systems (ASCII only)
/// </summary>
public class CpmCharacterEncoder : ICharacterEncoder
{
    /// <inheritdoc/>
    public MachineType SupportedMachine => MachineType.CpmGeneric;

    /// <inheritdoc/>
    public byte[] EncodeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<byte>();

        // CP/M uses uppercase for filenames
        var upperText = text.ToUpperInvariant();
        
        // Convert to ASCII, replacing non-ASCII characters with spaces
        var result = new byte[upperText.Length];
        for (int i = 0; i < upperText.Length; i++)
        {
            var ch = upperText[i];
            result[i] = (ch >= 0x20 && ch <= 0x7E) ? (byte)ch : (byte)0x20;
        }
        
        return result;
    }

    /// <inheritdoc/>
    public string DecodeText(byte[] data)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        // Simple ASCII decoding
        var chars = new char[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var b = data[i];
            chars[i] = (b >= 0x20 && b <= 0x7E) ? (char)b : ' ';
        }
        
        return new string(chars);
    }
}