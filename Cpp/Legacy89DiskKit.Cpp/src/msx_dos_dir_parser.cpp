#include "legacy89diskkit/cpp/msx_dos_dir_parser.hpp"

#include "legacy89diskkit/cpp/msx_dos_mode_rules.hpp"

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
        text.push_back(static_cast<char>(data[index]));
    }

    while (!text.empty() && text.back() == ' ')
    {
        text.pop_back();
    }

    return text;
}
}

MsxDosDirectoryEntry MsxDosDirParser::ParseEntry(const std::array<std::uint8_t, 32>& entry_data)
{
    MsxDosDirectoryEntry entry{};
    std::copy_n(entry_data.begin(), 8, entry.raw_file_name.begin());
    std::copy_n(entry_data.begin() + 8, 3, entry.raw_extension.begin());
    entry.file_name = DecodeTrimmed(entry.raw_file_name.data(), entry.raw_file_name.size());
    entry.extension = DecodeTrimmed(entry.raw_extension.data(), entry.raw_extension.size());
    entry.attribute_byte = entry_data[11];
    entry.write_time = static_cast<std::uint16_t>(entry_data[22] | (entry_data[23] << 8));
    entry.write_date = static_cast<std::uint16_t>(entry_data[24] | (entry_data[25] << 8));
    entry.start_cluster = static_cast<int>(entry_data[26] | (entry_data[27] << 8));
    entry.size = static_cast<std::uint32_t>(entry_data[28] |
                                            (entry_data[29] << 8) |
                                            (entry_data[30] << 16) |
                                            (entry_data[31] << 24));
    return entry;
}

MsxDosFileEntry MsxDosDirParser::ParseFileEntry(const std::array<std::uint8_t, 32>& entry_data)
{
    const auto entry = ParseEntry(entry_data);
    const auto attributes = MsxDosModeRules::Parse(entry.attribute_byte);
    return MsxDosFileEntry{
        entry.file_name,
        entry.extension,
        entry.size,
        attributes,
        entry.start_cluster,
        entry.write_time,
        entry.write_date,
        entry.raw_file_name,
        entry.raw_extension };
}

std::array<std::uint8_t, 32> MsxDosDirParser::Write(const MsxDosFileEntry& entry)
{
    std::array<std::uint8_t, 32> data{};
    data.fill(0x00);

    const auto name_count = std::min<std::size_t>(8, entry.file_name.size());
    for (std::size_t index = 0; index < name_count; ++index)
    {
        data[index] = static_cast<std::uint8_t>(entry.file_name[index]);
    }

    const auto extension_count = std::min<std::size_t>(3, entry.extension.size());
    for (std::size_t index = 0; index < extension_count; ++index)
    {
        data[8 + index] = static_cast<std::uint8_t>(entry.extension[index]);
    }

    data[11] = MsxDosModeRules::BuildAttributeByte(entry.attributes);
    data[22] = static_cast<std::uint8_t>(entry.write_time & 0xff);
    data[23] = static_cast<std::uint8_t>((entry.write_time >> 8) & 0xff);
    data[24] = static_cast<std::uint8_t>(entry.write_date & 0xff);
    data[25] = static_cast<std::uint8_t>((entry.write_date >> 8) & 0xff);
    data[26] = static_cast<std::uint8_t>(entry.start_cluster & 0xff);
    data[27] = static_cast<std::uint8_t>((entry.start_cluster >> 8) & 0xff);
    data[28] = static_cast<std::uint8_t>(entry.size & 0xff);
    data[29] = static_cast<std::uint8_t>((entry.size >> 8) & 0xff);
    data[30] = static_cast<std::uint8_t>((entry.size >> 16) & 0xff);
    data[31] = static_cast<std::uint8_t>((entry.size >> 24) & 0xff);
    return data;
}
}
