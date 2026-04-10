#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicDirectoryLayoutRules
{
public:
    static std::vector<std::vector<std::uint8_t>> BuildDirectorySectors(
        const std::vector<HuBasicFileEntry>& entries,
        int sector_size,
        int sector_count);
};
}
