#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicDirectoryListing
{
public:
    static std::vector<HuBasicFileEntry> ListFiles(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size);
};
}
