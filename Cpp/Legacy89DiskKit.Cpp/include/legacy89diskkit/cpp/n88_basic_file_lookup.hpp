#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicFileLookup
{
public:
    static std::optional<N88BasicFileEntry> FindByName(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config,
        const char* file_name);

    static bool Exists(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config,
        const char* file_name);
};
}
