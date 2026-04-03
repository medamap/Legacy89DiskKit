using Legacy89DiskKit.CharacterEncoding.Domain.Interface;
using Legacy89DiskKit.CharacterEncoding.Domain.Model;
using System.Text;

namespace Legacy89DiskKit.CharacterEncoding.Infrastructure.Encoder;

/// <summary>
/// Character encoder for MSX-DOS (CP/M compatible) with Shift-JIS support
/// </summary>
public class CpmMsxDosCharacterEncoder : ICharacterEncoder
{
    private static readonly Encoding ShiftJis;

    static CpmMsxDosCharacterEncoder()
    {
        // Register Shift-JIS encoding
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ShiftJis = Encoding.GetEncoding("shift_jis");
    }

    /// <inheritdoc/>
    public MachineType SupportedMachine => MachineType.CpmMsxDos;

    /// <inheritdoc/>
    public byte[] EncodeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<byte>();

        try
        {
            // MSX-DOS uses Shift-JIS encoding
            // For filenames, convert to uppercase
            var processedText = text.ToUpperInvariant();
            return ShiftJis.GetBytes(processedText);
        }
        catch
        {
            // Fallback to ASCII if Shift-JIS fails
            return new CpmCharacterEncoder().EncodeText(text);
        }
    }

    /// <inheritdoc/>
    public string DecodeText(byte[] data)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        try
        {
            // Try Shift-JIS decoding first
            return ShiftJis.GetString(data);
        }
        catch
        {
            // Fallback to ASCII if Shift-JIS fails
            return new CpmCharacterEncoder().DecodeText(data);
        }
    }
}