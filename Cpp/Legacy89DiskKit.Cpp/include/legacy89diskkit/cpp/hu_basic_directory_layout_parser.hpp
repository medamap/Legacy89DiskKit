#pragma once

#include "legacy89diskkit/cpp/hu_basic_directory_layout_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicDirectoryLayoutParser
{
public:
    static HuBasicDirectoryLayout Parse(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size);
};
}
