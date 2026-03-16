#include "legacy89diskkit/cpp/n88_basic_shell.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<N88BasicFileEntry> N88BasicShell::ListFiles(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config)
{
    return N88BasicDirectoryListing::ListFiles(directory_sectors, fat_data, config);
}

bool N88BasicShell::FileExists(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config,
    const char* file_name)
{
    const auto files = ListFiles(directory_sectors, fat_data, config);
    for (const auto& file : files)
    {
        if (HuBasicNameRules::BuildDisplayName(file.file_name, file.extension) == file_name)
        {
            return true;
        }
    }

    return false;
}
}
