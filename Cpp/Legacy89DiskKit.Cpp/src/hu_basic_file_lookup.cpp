#include "legacy89diskkit/cpp/hu_basic_file_lookup.hpp"

#include "legacy89diskkit/cpp/hu_basic_directory_listing.hpp"
#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
std::optional<HuBasicFileEntry> HuBasicFileLookup::FindByName(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* file_name)
{
    const auto files = HuBasicDirectoryListing::ListFiles(directory_sectors, sector_size);
    for (const auto& file : files)
    {
        if (HuBasicNameRules::BuildDisplayName(file.file_name, file.extension) == file_name)
        {
            return file;
        }
    }

    return std::nullopt;
}

bool HuBasicFileLookup::Exists(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* file_name)
{
    return FindByName(directory_sectors, sector_size, file_name).has_value();
}
}
