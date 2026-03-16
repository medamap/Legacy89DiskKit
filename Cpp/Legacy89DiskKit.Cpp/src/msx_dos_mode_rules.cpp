#include "legacy89diskkit/cpp/msx_dos_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
MsxDosFileAttributes MsxDosModeRules::Parse(const std::uint8_t attribute_byte)
{
    return MsxDosFileAttributes{
        (attribute_byte & 0x10) == 0,
        attribute_byte,
        (attribute_byte & 0x01) != 0,
        (attribute_byte & 0x02) != 0,
        (attribute_byte & 0x04) != 0,
        (attribute_byte & 0x10) != 0,
        (attribute_byte & 0x20) != 0 };
}

std::uint8_t MsxDosModeRules::BuildAttributeByte(const MsxDosFileAttributes& attributes)
{
    auto attribute_byte = static_cast<std::uint8_t>(0x00);
    if (attributes.is_read_only)
    {
        attribute_byte |= 0x01;
    }
    if (attributes.is_hidden)
    {
        attribute_byte |= 0x02;
    }
    if (attributes.is_system)
    {
        attribute_byte |= 0x04;
    }
    if (attributes.is_directory)
    {
        attribute_byte |= 0x10;
    }
    if (attributes.is_archive)
    {
        attribute_byte |= 0x20;
    }

    return attribute_byte;
}
}
