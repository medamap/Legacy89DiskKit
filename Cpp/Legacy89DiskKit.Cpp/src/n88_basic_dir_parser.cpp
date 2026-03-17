#include "legacy89diskkit/cpp/n88_basic_dir_parser.hpp"

#include "legacy89diskkit/cpp/n88_basic_mode_rules.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
namespace
{
std::string DecodeTrimmed(const std::uint8_t* data, const std::size_t length)
{
    std::string text;
    text.reserve(length);
    for (std::size_t index = 0; index < length; ++index)
    {
        const char ch = static_cast<char>(data[index]);
        if (ch == '\0') break;
        text.push_back(ch);
    }

    while (!text.empty() && text.back() == ' ')
    {
        text.pop_back();
    }

    return text;
}
}

N88BasicDirectoryEntry N88BasicDirParser::ParseEntry(const std::array<std::uint8_t, 16>& entry_data)
{
    N88BasicDirectoryEntry entry{};
    std::copy_n(entry_data.begin(), 6, entry.raw_file_name.begin());
    std::copy_n(entry_data.begin() + 6, 3, entry.raw_extension.begin());
    entry.file_name = DecodeTrimmed(entry.raw_file_name.data(), entry.raw_file_name.size());
    entry.extension = DecodeTrimmed(entry.raw_extension.data(), entry.raw_extension.size());
    entry.attribute_byte = entry_data[9];
    entry.start_cluster = entry_data[10];
    return entry;
}

N88BasicFileEntry N88BasicDirParser::ParseFileEntry(const std::array<std::uint8_t, 16>& entry_data)
{
    const auto entry = ParseEntry(entry_data);
    const auto is_ascii = N88BasicModeRules::IsAscii(entry.attribute_byte);

    return N88BasicFileEntry
    {
        entry.file_name,
        entry.extension,
        0,
        N88BasicFileAttributes
        {
            is_ascii,
            entry.attribute_byte,
            (entry.attribute_byte & 0x10) != 0
        },
        entry.start_cluster
    };
}

std::array<std::uint8_t, 16> N88BasicDirParser::Write(const N88BasicFileEntry& entry)
{
    std::array<std::uint8_t, 16> data{};
    data.fill(0x00);

    const auto name_count = std::min<std::size_t>(6, entry.file_name.size());
    for (std::size_t index = 0; index < name_count; ++index)
    {
        data[index] = static_cast<std::uint8_t>(entry.file_name[index]);
    }

    const auto ext_count = std::min<std::size_t>(3, entry.extension.size());
    for (std::size_t index = 0; index < ext_count; ++index)
    {
        data[6 + index] = static_cast<std::uint8_t>(entry.extension[index]);
    }

    data[9] = entry.attributes.raw_attributes;
    data[10] = static_cast<std::uint8_t>(entry.start_cluster);
    return data;
}
}
