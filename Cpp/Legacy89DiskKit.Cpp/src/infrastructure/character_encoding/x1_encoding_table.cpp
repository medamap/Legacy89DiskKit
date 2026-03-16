#include "legacy89diskkit/cpp/infrastructure/character_encoding/x1_encoding_table.hpp"

namespace legacy89diskkit::cpp
{
const ByteTextEncodingTable& X1EncodingTable::Get()
{
    static const ByteTextEncodingTable table{
        "x1",
        []()
        {
            std::array<std::string_view, 256> values{};

            values[0x0d] = "\n";
            values[0x1a] = "\x1A";

            for (int value = 0x20; value <= 0x7e; ++value)
            {
                values[static_cast<std::size_t>(value)] =
                    std::string_view(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~")
                        .substr(static_cast<std::size_t>(value - 0x20), 1);
            }

            values[0x7f] = "π";
            values[0x1c] = "→";
            values[0x1d] = "←";
            values[0x1e] = "↑";
            values[0x1f] = "↓";
            values[0x80] = " ";
            values[0x81] = "▂";
            values[0x82] = "▃";
            values[0x83] = "▄";
            values[0x84] = "▅";
            values[0x85] = "▆";
            values[0x86] = "▇";
            values[0x87] = "█";
            values[0x88] = "▏";
            values[0x89] = "▎";
            values[0x8a] = "▍";
            values[0x8b] = "▌";
            values[0x8c] = "▋";
            values[0x8d] = "▊";
            values[0x8e] = "▉";
            values[0x8f] = "／";
            values[0x90] = "─";
            values[0x91] = "│";
            values[0x92] = "┴";
            values[0x93] = "┬";
            values[0x94] = "┤";
            values[0x95] = "├";
            values[0x96] = "┼";
            values[0x97] = "┐";
            values[0x98] = "┘";
            values[0x99] = "└";
            values[0x9a] = "┌";
            values[0x9b] = "◟";
            values[0x9c] = "◝";
            values[0x9d] = "◜";
            values[0x9e] = "◞";
            values[0x9f] = "＼";
            values[0xa0] = " ";
            values[0xa1] = "｡";
            values[0xa2] = "｢";
            values[0xa3] = "｣";
            values[0xa4] = "､";
            values[0xa5] = "･";

            constexpr std::string_view katakana[] = {
                "ｦ","ｧ","ｨ","ｩ","ｪ","ｫ","ｬ","ｭ","ｮ","ｯ","ｰ","ｱ","ｲ","ｳ","ｴ","ｵ","ｶ","ｷ","ｸ","ｹ","ｺ",
                "ｻ","ｼ","ｽ","ｾ","ｿ","ﾀ","ﾁ","ﾂ","ﾃ","ﾄ","ﾅ","ﾆ","ﾇ","ﾈ","ﾉ","ﾊ","ﾋ","ﾌ","ﾍ","ﾎ","ﾏ","ﾐ",
                "ﾑ","ﾒ","ﾓ","ﾔ","ﾕ","ﾖ","ﾗ","ﾘ","ﾙ","ﾚ","ﾛ","ﾜ","ﾝ","ﾞ","ﾟ"," "," "," "," "};
            for (std::size_t i = 0; i < std::size(katakana); ++i)
            {
                values[0xa6 + i] = katakana[i];
            }

            values[0xe0] = "●";
            values[0xe1] = "○";
            values[0xe2] = "♠";
            values[0xe3] = "♥";
            values[0xe4] = "♦";
            values[0xe5] = "♣";
            values[0xe6] = "▲";
            values[0xe7] = "▼";
            values[0xe8] = "×";
            values[0xe9] = "■";
            values[0xea] = "□";
            values[0xeb] = "█";
            values[0xec] = "▓";
            values[0xed] = "▒";
            values[0xee] = "░";
            values[0xef] = "□";
            values[0xf0] = "✓";
            values[0xf1] = "土";
            values[0xf2] = "金";
            values[0xf3] = "木";
            values[0xf4] = "水";
            values[0xf5] = "火";
            values[0xf6] = "月";
            values[0xf7] = "日";
            values[0xf8] = "時";
            values[0xf9] = "分";
            values[0xfa] = "秒";
            values[0xfb] = "年";
            values[0xfc] = "円";
            values[0xfd] = "人";
            values[0xfe] = "生";
            values[0xff] = "〒";

            return values;
        }()};

    return table;
}
}
