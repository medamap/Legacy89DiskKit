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

std::optional<MsxDosWriteTransactionPlan> MsxDosShell::PlanWrite(
    const char* file_name,
    const std::vector<std::uint8_t>& data,
    const MsxDosFileAttributes& attributes,
    const MsxDosConfiguration& config,
    const std::vector<std::uint8_t>& fat_data)
{
    return MsxDosWriteTransaction::CreatePlan(file_name, data, attributes, config, fat_data);
}

std::optional<MsxDosDeleteTransactionPlan> MsxDosShell::PlanDelete(
    const std::vector<std::uint8_t>& fat_data,
    const std::vector<int>& clusters,
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const MsxDosConfiguration& config,
    const char* file_name)
{
    return MsxDosDeleteTransaction::CreatePlan(fat_data, clusters, directory_sectors, config, file_name);
}

std::optional<MsxDosRenameTransactionPlan> MsxDosShell::PlanRename(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const MsxDosConfiguration& config,
    const char* old_name,
    const char* new_name)
{
    return MsxDosRenameTransaction::CreatePlan(directory_sectors, config, old_name, new_name);
}

std::optional<MsxDosAttributeUpdateTransactionPlan> MsxDosShell::PlanAttributeUpdate(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const MsxDosConfiguration& config,
    const char* file_name,
    const MsxDosFileAttributes& attributes)
{
    return MsxDosAttributeUpdateTransaction::CreatePlan(directory_sectors, config, file_name, attributes);
}

std::vector<std::uint8_t> MsxDosShell::CreateFatData(const MsxDosConfiguration& config)
{
    return MsxDosFormatRules::CreateFatData(config);
}

std::vector<std::vector<std::uint8_t>> MsxDosShell::CreateRootDirectorySectors(const MsxDosConfiguration& config)
{
    return MsxDosFormatRules::CreateRootDirectorySectors(config);
}
}
