#include "legacy89diskkit/cpp/hu_basic_directory_sector_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

#include <algorithm>
#include <array>
#include <cctype>

namespace legacy89diskkit::cpp
{
namespace
{
std::string ToUpper(std::string value)
{
    std::transform(
        value.begin(),
        value.end(),
        value.begin(),
        [](const unsigned char ch)
        {
            return static_cast<char>(std::toupper(ch));
        });
    return value;
}

std::string GetFullName(const HuBasicFileEntry& entry)
{
    return HuBasicNameRules::BuildDisplayName(entry.file_name, entry.extension);
}
}

std::optional<int> HuBasicDirectorySectorRules::FindWritableSlotOffset(
    const std::vector<std::uint8_t>& sector_data,
    const int sector_size)
{
    for (int offset = 0; offset < sector_size; offset += 32)
    {
        const auto mode = sector_data[offset];
        if (mode == 0x00 || mode == 0xff)
        {
            return offset;
        }
    }

    return std::nullopt;
}

std::optional<int> HuBasicDirectorySectorRules::FindEntryOffset(
    const std::vector<std::uint8_t>& sector_data,
    const int sector_size,
    const std::string& full_name)
{
    const auto normalized_name = ToUpper(full_name);

    for (int offset = 0; offset < sector_size; offset += 32)
    {
        const auto mode = sector_data[offset];
        if (mode == 0xff)
        {
            return std::nullopt;
        }

        if (mode == 0x00 || mode == 0xe5)
        {
            continue;
        }

        std::array<std::uint8_t, 32> entry_bytes{};
        std::copy_n(sector_data.begin() + offset, 32, entry_bytes.begin());
        const auto entry = HuBasicDirParser::Parse(HuBasicDirectoryEntryCodec::Parse(entry_bytes));

        if (ToUpper(GetFullName(entry)) == normalized_name)
        {
            return offset;
        }
    }

    return std::nullopt;
}

int HuBasicDirectorySectorRules::CountActiveEntries(const std::vector<std::uint8_t>& sector_data, const int sector_size)
{
    auto count = 0;

    for (int offset = 0; offset < sector_size; offset += 32)
    {
        const auto mode = sector_data[offset];
        if (mode == 0xff)
        {
            break;
        }

        if (mode != 0x00 && mode != 0xe5)
        {
            ++count;
        }
    }

    return count;
}

void HuBasicDirectorySectorRules::MarkEntryDeleted(std::vector<std::uint8_t>& sector_data, const int offset)
{
    sector_data[offset] = 0x00;
}
}
