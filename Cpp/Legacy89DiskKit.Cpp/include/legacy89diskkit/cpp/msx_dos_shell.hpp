#pragma once

#include "legacy89diskkit/cpp/msx_dos_directory_listing.hpp"
#include "legacy89diskkit/cpp/msx_dos_file_lookup.hpp"
#include "legacy89diskkit/cpp/msx_dos_filesystem_info_rules.hpp"

namespace legacy89diskkit::cpp
{
class MsxDosShell
{
public:
    static std::vector<MsxDosFileEntry> ListFiles(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config);

    static bool FileExists(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config,
        const char* file_name);

    static std::optional<MsxDosFileEntry> FindFile(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config,
        const char* file_name);

    static MsxDosFileSystemInfo GetFileSystemInfo(
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config);
};
}
