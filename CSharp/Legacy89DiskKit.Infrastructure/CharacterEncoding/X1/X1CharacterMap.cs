namespace Legacy89DiskKit.Infrastructure.CharacterEncoding.X1;

public static class X1CharacterMap
{
    public static readonly string[] ByteToText;
    public static readonly IReadOnlyDictionary<string, byte> TextToByte;

    static X1CharacterMap()
    {
        var byteToText = new string[256];
        var textToByte = new Dictionary<string, byte>();

        for (int i = 0; i < 256; i++)
        {
            byteToText[i] = "";
        }

        byteToText[0x0D] = "\n";
        byteToText[0x1A] = "";

        for (byte i = 0; i <= 0x5E; i++)
        {
            byte code = (byte)(0x20 + i);
            byteToText[code] = ((char)code).ToString();
        }
        byteToText[0x7F] = "π";

        byteToText[0x1C] = "→";
        byteToText[0x1D] = "←";
        byteToText[0x1E] = "↑";
        byteToText[0x1F] = "↓";

        byteToText[0x9B] = "◟";
        byteToText[0x9C] = "◝";
        byteToText[0x9D] = "◜";
        byteToText[0x9E] = "◞";

        byteToText[0xA0] = " ";
        byteToText[0xA1] = "｡";
        byteToText[0xA2] = "｢";
        byteToText[0xA3] = "｣";
        byteToText[0xA4] = "､";
        byteToText[0xA5] = "･";
        for (int i = 0; i < 63; i++)
        {
            byteToText[0xA6 + i] = ((char)(0xFF66 + i)).ToString();
        }

        byteToText[0xE0] = "●";
        byteToText[0xE1] = "○";
        byteToText[0xE2] = "♠";
        byteToText[0xE3] = "♥";
        byteToText[0xE4] = "♦";
        byteToText[0xE5] = "♣";
        byteToText[0xE6] = "▲";
        byteToText[0xE7] = "▼";
        byteToText[0xE8] = "×";
        byteToText[0xE9] = "■";
        byteToText[0xEA] = "□";
        byteToText[0xEB] = "█";
        byteToText[0xEC] = "▓";
        byteToText[0xED] = "▒";
        byteToText[0xEE] = "░";
        byteToText[0xEF] = "□";
        byteToText[0xF0] = "✓";
        byteToText[0xF1] = "土";
        byteToText[0xF2] = "金";
        byteToText[0xF3] = "木";
        byteToText[0xF4] = "水";
        byteToText[0xF5] = "火";
        byteToText[0xF6] = "月";
        byteToText[0xF7] = "日";
        byteToText[0xF8] = "時";
        byteToText[0xF9] = "分";
        byteToText[0xFA] = "秒";
        byteToText[0xFB] = "年";
        byteToText[0xFC] = "円";
        byteToText[0xFD] = "人";
        byteToText[0xFE] = "生";
        byteToText[0xFF] = "〒";

        byteToText[0x80] = " ";
        byteToText[0x81] = "▂";
        byteToText[0x82] = "▃";
        byteToText[0x83] = "▄";
        byteToText[0x84] = "▅";
        byteToText[0x85] = "▆";
        byteToText[0x86] = "▇";
        byteToText[0x87] = "█";
        byteToText[0x88] = "▏";
        byteToText[0x89] = "▎";
        byteToText[0x8A] = "▍";
        byteToText[0x8B] = "▌";
        byteToText[0x8C] = "▋";
        byteToText[0x8D] = "▊";
        byteToText[0x8E] = "▉";
        byteToText[0x8F] = "／";
        byteToText[0x90] = "─";
        byteToText[0x91] = "│";
        byteToText[0x92] = "┴";
        byteToText[0x93] = "┬";
        byteToText[0x94] = "┤";
        byteToText[0x95] = "├";
        byteToText[0x96] = "┼";
        byteToText[0x97] = "┐";
        byteToText[0x98] = "┘";
        byteToText[0x99] = "└";
        byteToText[0x9A] = "┌";
        byteToText[0x9F] = "＼";

        for (int i = 0; i < 256; i++)
        {
            string text = byteToText[i];
            if (!string.IsNullOrEmpty(text) && !textToByte.ContainsKey(text))
            {
                textToByte[text] = (byte)i;
            }
        }

        textToByte["\n"] = 0x0D;
        textToByte["\r"] = 0x0D;
        byteToText[0x1A] = "\x1A";
        textToByte["\x1A"] = 0x1A;

        ByteToText = byteToText;
        TextToByte = textToByte;
    }
}
