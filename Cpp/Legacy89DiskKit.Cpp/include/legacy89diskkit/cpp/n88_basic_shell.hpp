#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/n88_basic_directory_listing.hpp"

namespace legacy89diskkit::cpp
{
class N88BasicShell
{
public:
    static std::vector<N88BasicFileEntry> ListFiles(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config);

    static bool FileExists(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config,
        const char* file_name);
};
}
