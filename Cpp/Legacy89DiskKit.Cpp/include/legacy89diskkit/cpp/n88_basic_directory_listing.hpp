#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicDirectoryListing
{
public:
    static std::vector<N88BasicFileEntry> ListFiles(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config);
};
}
