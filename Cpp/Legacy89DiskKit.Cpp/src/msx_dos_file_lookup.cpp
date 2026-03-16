#include "legacy89diskkit/cpp/msx_dos_file_lookup.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_directory_listing.hpp"

namespace legacy89diskkit::cpp
{
std::optional<MsxDosFileEntry> MsxDosFileLookup::FindByName(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config,
    const char* file_name)
{
    const auto files = MsxDosDirectoryListing::ListFiles(directory_sectors, fat_data, config);
    for (const auto& file : files)
    {
        if (HuBasicNameRules::BuildDisplayName(file.file_name, file.extension) == file_name)
        {
            return file;
        }
    }

    return std::nullopt;
}

bool MsxDosFileLookup::Exists(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config,
    const char* file_name)
{
    return FindByName(directory_sectors, fat_data, config, file_name).has_value();
}
}
