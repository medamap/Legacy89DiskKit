#include "legacy89diskkit/cpp/hu_basic_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicFileType HuBasicModeRules::GetFileType(const std::uint8_t mode_byte)
{
    if ((mode_byte & 0x01) != 0)
    {
        return HuBasicFileType::Binary;
    }

    if ((mode_byte & 0x02) != 0)
    {
        return HuBasicFileType::Basic;
    }

    if ((mode_byte & 0x0c) != 0)
    {
        return HuBasicFileType::Ascii;
    }

    return HuBasicFileType::Unknown;
}

std::uint8_t HuBasicModeRules::BuildModeByte(const HuBasicFileMetadata& metadata)
{
    std::uint8_t mode_byte = 0;

    if (metadata.is_directory)
    {
        mode_byte |= 0x80;
    }

    if (metadata.is_write_protected)
    {
        mode_byte |= 0x40;
    }

    if (metadata.is_verify)
    {
        mode_byte |= 0x20;
    }

    if (metadata.is_hidden)
    {
        mode_byte |= 0x10;
    }

    switch (metadata.file_type)
    {
    case HuBasicFileType::Binary:
        mode_byte |= 0x01;
        break;
    case HuBasicFileType::Basic:
        mode_byte |= 0x02;
        break;
    case HuBasicFileType::Ascii:
        mode_byte |= 0x04;
        break;
    case HuBasicFileType::Unknown:
        break;
    }

    return mode_byte;
}

std::uint8_t HuBasicModeRules::BuildModeByte(const HuBasicFileAttributes& attributes)
{
    auto mode_byte = static_cast<std::uint8_t>(attributes.raw_attributes & 0x0f);

    if (attributes.is_directory)
    {
        mode_byte |= 0x80;
    }

    if (attributes.is_read_only)
    {
        mode_byte |= 0x40;
    }

    if (attributes.is_hidden)
    {
        mode_byte |= 0x10;
    }

    if ((mode_byte & 0x0f) == 0)
    {
        mode_byte |= attributes.is_ascii ? static_cast<std::uint8_t>(0x04) : static_cast<std::uint8_t>(0x01);
    }

    return mode_byte;
}
}
