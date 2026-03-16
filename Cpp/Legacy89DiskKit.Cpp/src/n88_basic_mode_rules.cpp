#include "legacy89diskkit/cpp/n88_basic_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
bool N88BasicModeRules::IsAscii(const std::uint8_t attribute_byte)
{
    return (attribute_byte & 0x80) == 0 && (attribute_byte & 0x01) == 0;
}

bool N88BasicModeRules::IsBinary(const std::uint8_t attribute_byte)
{
    return (attribute_byte & 0x01) != 0;
}

std::uint8_t N88BasicModeRules::BuildAttributeByte(const N88BasicFileAttributes& attributes)
{
    auto attribute_byte = attributes.is_ascii ? 0x00 : 0x01;
    if (attributes.is_read_only)
    {
        attribute_byte |= 0x10;
    }

    return static_cast<std::uint8_t>(attribute_byte);
}
}
