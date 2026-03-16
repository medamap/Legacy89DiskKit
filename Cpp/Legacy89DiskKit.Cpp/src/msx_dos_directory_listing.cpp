#include "legacy89diskkit/cpp/msx_dos_directory_listing.hpp"

#include "legacy89diskkit/cpp/msx_dos_dir_parser.hpp"
#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_read_rules.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
std::vector<MsxDosFileEntry> MsxDosDirectoryListing::ListFiles(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config)
{
    std::vector<MsxDosFileEntry> files;

    for (const auto& sector : directory_sectors)
    {
        for (auto offset = 0; offset < config.sector_size; offset += 32)
        {
            const auto marker = sector[offset];
            if (marker == 0x00)
            {
                return files;
            }

            if (marker == 0xe5)
            {
                continue;
            }

            std::array<std::uint8_t, 32> entry_bytes{};
            std::copy_n(sector.begin() + offset, 32, entry_bytes.begin());
            auto file = MsxDosDirParser::ParseFileEntry(entry_bytes);
            const auto chain = MsxDosFatRules::GetClusterChain(fat_data, config, file.start_cluster);
            file.size = MsxDosReadRules::ResolveSizeFromFat(chain, config, file.size);
            files.push_back(file);
        }
    }

    return files;
}
}
