#include "legacy89diskkit/cpp/hu_basic_boot_record_parser.hpp"

#include <algorithm>
#include <string_view>

namespace legacy89diskkit::cpp
{
namespace
{
std::string DecodePaddedText(const std::vector<std::uint8_t>& data, const std::size_t offset, const std::size_t length)
{
    std::string text;
    text.reserve(length);

    for (std::size_t index = 0; index < length; ++index)
    {
        const char ch = static_cast<char>(data[offset + index]);
        if (ch == '\0') break;
        text.push_back(ch);
    }

    while (!text.empty() && text.back() == ' ')
    {
        text.pop_back();
    }

    return text;
}

std::uint16_t ReadUInt16(const std::vector<std::uint8_t>& data, const std::size_t offset)
{
    return static_cast<std::uint16_t>(data[offset] | (static_cast<std::uint16_t>(data[offset + 1]) << 8));
}
}

std::optional<HuBasicBootRecordInfo> HuBasicBootRecordParser::Parse(const std::vector<std::uint8_t>& boot_area)
{
    if (boot_area.size() < 32 || boot_area[0] == 0x00)
    {
        return std::nullopt;
    }

    return HuBasicBootRecordInfo
    {
        boot_area[0],
        DecodePaddedText(boot_area, 1, 13),
        DecodePaddedText(boot_area, 0x0e, 3),
        boot_area[0x11] != 0x20,
        ReadUInt16(boot_area, 0x12),
        ReadUInt16(boot_area, 0x14),
        ReadUInt16(boot_area, 0x16),
        ReadUInt16(boot_area, 0x1e)
    };
}
}
