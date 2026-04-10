#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"
#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosDirectoryListing
{
public:
    static std::vector<MsxDosFileEntry> ListFiles(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config);
};
}
