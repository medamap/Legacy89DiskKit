#include "legacy89diskkit/cpp/msx_dos_shell.hpp"

namespace legacy89diskkit::cpp
{
std::vector<MsxDosFileEntry> MsxDosShell::ListFiles(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config)
{
    return MsxDosDirectoryListing::ListFiles(directory_sectors, fat_data, config);
}

bool MsxDosShell::FileExists(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config,
    const char* file_name)
{
    return MsxDosFileLookup::Exists(directory_sectors, fat_data, config, file_name);
}

std::optional<MsxDosFileEntry> MsxDosShell::FindFile(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config,
    const char* file_name)
{
    return MsxDosFileLookup::FindByName(directory_sectors, fat_data, config, file_name);
}

MsxDosFileSystemInfo MsxDosShell::GetFileSystemInfo(
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config)
{
    return MsxDosFileSystemInfoRules::BuildInfo(fat_data, config);
}
}
