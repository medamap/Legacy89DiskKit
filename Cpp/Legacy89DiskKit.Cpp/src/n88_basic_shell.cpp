#include "legacy89diskkit/cpp/n88_basic_shell.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_format_rules.hpp"

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
    return N88BasicFileLookup::Exists(directory_sectors, fat_data, config, file_name);
}

std::optional<N88BasicFileEntry> N88BasicShell::FindFile(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config,
    const char* file_name)
{
    return N88BasicFileLookup::FindByName(directory_sectors, fat_data, config, file_name);
}

N88BasicFileSystemInfo N88BasicShell::GetFileSystemInfo(
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config)
{
    return N88BasicFileSystemInfoRules::BuildInfo(fat_data, config);
}

std::optional<N88BasicWriteTransactionPlan> N88BasicShell::PlanWrite(
    const char* file_name,
    const std::vector<std::uint8_t>& data,
    const N88BasicFileAttributes& attributes,
    const N88BasicConfiguration& config,
    const std::vector<std::uint8_t>& fat_data)
{
    return N88BasicWriteTransaction::CreatePlan(file_name, data, attributes, config, fat_data);
}

std::optional<N88BasicDeleteTransactionPlan> N88BasicShell::PlanDelete(
    const std::vector<std::uint8_t>& fat_data,
    const std::vector<int>& clusters,
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const N88BasicConfiguration& config,
    const char* file_name)
{
    return N88BasicDeleteTransaction::CreatePlan(
        fat_data,
        clusters,
        directory_sectors,
        config,
        file_name);
}

std::optional<N88BasicRenameTransactionPlan> N88BasicShell::PlanRename(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const N88BasicConfiguration& config,
    const char* old_name,
    const char* new_name)
{
    return N88BasicRenameTransaction::CreatePlan(
        directory_sectors,
        config,
        old_name,
        new_name);
}

std::optional<N88BasicAttributeUpdateTransactionPlan> N88BasicShell::PlanAttributeUpdate(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const N88BasicConfiguration& config,
    const char* file_name,
    const N88BasicFileAttributes& attributes)
{
    return N88BasicAttributeUpdateTransaction::CreatePlan(
        directory_sectors,
        config,
        file_name,
        attributes);
}

std::vector<std::uint8_t> N88BasicShell::CreateFatData(const N88BasicConfiguration& config)
{
    return N88BasicFormatRules::CreateFatData(config);
}

std::vector<std::vector<std::uint8_t>> N88BasicShell::CreateDirectorySectors(const N88BasicConfiguration& config)
{
    return N88BasicFormatRules::CreateDirectorySectors(config);
}
}
