#include "legacy89diskkit/cpp/hu_basic_directory_layout_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_file_entry_writer.hpp"

#include <algorithm>
#include <stdexcept>

namespace legacy89diskkit::cpp
{
std::vector<std::vector<std::uint8_t>> HuBasicDirectoryLayoutRules::BuildDirectorySectors(
    const std::vector<HuBasicFileEntry>& entries,
    const int sector_size,
    const int sector_count)
{
    const auto capacity = sector_count * (sector_size / 32);
    if (static_cast<int>(entries.size()) >= capacity)
    {
        throw std::invalid_argument("Directory layout exceeds capacity.");
    }

    std::vector<std::vector<std::uint8_t>> sectors(
        sector_count,
        std::vector<std::uint8_t>(sector_size, 0x00));

    auto current_sector = 0;
    auto current_offset = 0;

    for (const auto& entry : entries)
    {
        if (current_offset >= sector_size)
        {
            ++current_sector;
            current_offset = 0;
        }

        const auto raw_entry = HuBasicFileEntryWriter::ToDirectoryEntry(entry);
        const auto encoded = HuBasicDirectoryEntryCodec::Write(raw_entry);
        std::copy(encoded.begin(), encoded.end(), sectors[current_sector].begin() + current_offset);
        current_offset += 32;
    }

    if (current_sector < sector_count)
    {
        if (current_offset >= sector_size)
        {
            ++current_sector;
            current_offset = 0;
        }

        if (current_sector < sector_count)
        {
            sectors[current_sector][current_offset] = 0xff;
        }
    }

    return sectors;
}
}
