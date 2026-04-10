#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicFileLookup
{
public:
    static std::optional<HuBasicFileEntry> FindByName(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* file_name);

    static bool Exists(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* file_name);
};
}
