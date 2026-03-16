#include "legacy89diskkit/cpp/hu_basic_directory_listing.hpp"

#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
std::vector<HuBasicFileEntry> HuBasicDirectoryListing::ListFiles(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size)
{
    std::vector<HuBasicFileEntry> files;

    for (const auto& sector : directory_sectors)
    {
        for (auto offset = 0; offset < sector_size; offset += 32)
        {
            const auto mode = sector[offset];
            if (mode == 0xff)
            {
                return files;
            }

            if (mode == 0x00 || mode == 0xe5)
            {
                continue;
            }

            std::array<std::uint8_t, 32> entry_bytes{};
            std::copy_n(sector.begin() + offset, 32, entry_bytes.begin());
            files.push_back(HuBasicDirParser::Parse(HuBasicDirectoryEntryCodec::Parse(entry_bytes)));
        }
    }

    return files;
}
}
