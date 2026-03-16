#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosFileLookup
{
public:
    static std::optional<MsxDosFileEntry> FindByName(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config,
        const char* file_name);

    static bool Exists(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config,
        const char* file_name);
};
}
