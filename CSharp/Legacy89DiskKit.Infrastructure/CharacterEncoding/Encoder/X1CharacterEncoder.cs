using System.Text;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Model;

namespace Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;

public class X1CharacterEncoder : ICharacterEncoder
{
    public string EncodingId => "X1";
    public MachineType SupportedMachine => MachineType.X1;

    // Mapping from X1 byte to UTF-8 string
    private static readonly string[] X1ToStringMap = new string[256];
    private static readonly Dictionary<string, byte> StringToX1Map = new Dictionary<string, byte>();

    static X1CharacterEncoder()
    {
        InitializeMapping();
    }

    private static void InitializeMapping()
    {
        // Default to placeholders
        for (int i = 0; i < 256; i++) X1ToStringMap[i] = "";

        // Control codes
        X1ToStringMap[0x0D] = "\n"; // Mapping CR to LF for internal string representation
        X1ToStringMap[0x1A] = "";   // EOF usually ignored in strings

        // Lower ASCII (0x20-0x7E)
        for (byte i = 0; i <= 0x5E; i++)
        {
           byte code = (byte)(0x20 + i);
           char ch = (char)code;
           X1ToStringMap[code] = ch.ToString();
        }
        X1ToStringMap[0x7F] = "π";

        // Arrows (0x1C - 0x1F) - Based on x1disktool
        X1ToStringMap[0x1C] = "→";
        X1ToStringMap[0x1D] = "←";
        X1ToStringMap[0x1E] = "↑";
        X1ToStringMap[0x1F] = "↓";

        // Quad arcs (0x9B - 0x9E)
        X1ToStringMap[0x9B] = "◟"; // Lower Left
        X1ToStringMap[0x9C] = "◝"; // Upper Right
        X1ToStringMap[0x9D] = "◜"; // Upper Left
        X1ToStringMap[0x9E] = "◞"; // Lower Right

        // Katakana (0xA0-0xDF) - Half-width Katakana
        // Katakana (0xA0-0xDF) - Half-width Katakana
        // 0xA0 is space (0x20 replacement)
        X1ToStringMap[0xA0] = " ";
        X1ToStringMap[0xA1] = "｡";
        X1ToStringMap[0xA2] = "｢";
        X1ToStringMap[0xA3] = "｣";
        X1ToStringMap[0xA4] = "､";
        X1ToStringMap[0xA5] = "･";
        for (int i = 0; i < 63; i++)
        {
            X1ToStringMap[0xA6 + i] = ((char)(0xFF66 + i)).ToString();
        }

        // Symbols (0xE0-0xFF)
        X1ToStringMap[0xE0] = "●";
        X1ToStringMap[0xE1] = "○";
        X1ToStringMap[0xE2] = "♠";
        X1ToStringMap[0xE3] = "♥";
        X1ToStringMap[0xE4] = "♦";
        X1ToStringMap[0xE5] = "♣";
        X1ToStringMap[0xE6] = "▲"; // Triangle UP
        X1ToStringMap[0xE7] = "▼"; // Triangle DOWN
        X1ToStringMap[0xE8] = "×";
        X1ToStringMap[0xE9] = "■"; // Square 1 (Solid)
        X1ToStringMap[0xEA] = "□"; // Square 2 (Outline)
        // ... E9-EE are various shaded squares in X1, mapping them to standard blocks
        X1ToStringMap[0xEB] = "█"; 
        X1ToStringMap[0xEC] = "▓";
        X1ToStringMap[0xED] = "▒";
        X1ToStringMap[0xEE] = "░";
        X1ToStringMap[0xEF] = "□";

        X1ToStringMap[0xF0] = "✓";
        X1ToStringMap[0xF1] = "土";
        X1ToStringMap[0xF2] = "金";
        X1ToStringMap[0xF3] = "木";
        X1ToStringMap[0xF4] = "水";
        X1ToStringMap[0xF5] = "火";
        X1ToStringMap[0xF6] = "月";
        X1ToStringMap[0xF7] = "日";
        X1ToStringMap[0xF8] = "時";
        X1ToStringMap[0xF9] = "分";
        X1ToStringMap[0xFA] = "秒";
        X1ToStringMap[0xFB] = "年";
        X1ToStringMap[0xFC] = "円";
        X1ToStringMap[0xFD] = "人";
        X1ToStringMap[0xFE] = "生";
        X1ToStringMap[0xFF] = "〒";

        // Graphics and others (simplified mapping)
        // 0x80-0x87: Growing block (bottom up)
        X1ToStringMap[0x80] = " ";
        X1ToStringMap[0x81] = "▂";
        X1ToStringMap[0x82] = "▃";
        X1ToStringMap[0x83] = "▄";
        X1ToStringMap[0x84] = "▅";
        X1ToStringMap[0x85] = "▆";
        X1ToStringMap[0x86] = "▇";
        X1ToStringMap[0x87] = "█";

        // 0x88-0x8E: Growing block (left to right)
        X1ToStringMap[0x88] = "▏";
        X1ToStringMap[0x89] = "▎";
        X1ToStringMap[0x8A] = "▍";
        X1ToStringMap[0x8B] = "▌";
        X1ToStringMap[0x8C] = "▋";
        X1ToStringMap[0x8D] = "▊";
        X1ToStringMap[0x8E] = "▉";

        X1ToStringMap[0x8F] = "／";
        X1ToStringMap[0x90] = "─";
        X1ToStringMap[0x91] = "│";
        X1ToStringMap[0x92] = "┴";
        X1ToStringMap[0x93] = "┬";
        X1ToStringMap[0x94] = "┤";
        X1ToStringMap[0x95] = "├";
        X1ToStringMap[0x96] = "┼";
        X1ToStringMap[0x97] = "┐";
        X1ToStringMap[0x98] = "┘";
        X1ToStringMap[0x99] = "└";
        X1ToStringMap[0x9A] = "┌";
        X1ToStringMap[0x9F] = "＼";

        // Build reverse map
        for (int i = 0; i < 256; i++)
        {
            string s = X1ToStringMap[i];
            if (!string.IsNullOrEmpty(s) && !StringToX1Map.ContainsKey(s))
            {
                StringToX1Map[s] = (byte)i;
            }
        }

        // Explicitly map both \n and \r to 0x0D for encoding
        StringToX1Map["\n"] = 0x0D;
        StringToX1Map["\r"] = 0x0D;

        // Map EOF explicitly
        X1ToStringMap[0x1A] = "\x1A";
        StringToX1Map["\x1A"] = 0x1A;
    }

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
                sb.Append(X1ToStringMap[b]);
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
            if (StringToX1Map.TryGetValue(s, out var b))
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
