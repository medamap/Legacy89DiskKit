#include "legacy89diskkit/cpp/n88_basic_directory_listing.hpp"

#include "legacy89diskkit/cpp/n88_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_read_rules.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
std::vector<N88BasicFileEntry> N88BasicDirectoryListing::ListFiles(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config)
{
    std::vector<N88BasicFileEntry> files;

    for (const auto& sector : directory_sectors)
    {
        for (auto offset = 0; offset < config.sector_size; offset += 16)
        {
            const auto mode = sector[offset];
            if (mode == 0xff)
            {
                return files;
            }

            if (mode == 0x00)
            {
                continue;
            }

            std::array<std::uint8_t, 16> entry_bytes{};
            std::copy_n(sector.begin() + offset, 16, entry_bytes.begin());
            auto entry = N88BasicDirParser::ParseFileEntry(entry_bytes);
            const auto clusters = N88BasicFatRules::GetClusterChain(fat_data, config, entry.start_cluster);
            entry.size = N88BasicReadRules::ResolveSizeFromFat(clusters, fat_data, config);
            files.push_back(entry);
        }
    }

    return files;
}
}
