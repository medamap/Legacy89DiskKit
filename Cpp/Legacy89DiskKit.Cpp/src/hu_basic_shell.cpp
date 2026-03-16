#include "legacy89diskkit/cpp/hu_basic_shell.hpp"

#include "legacy89diskkit/cpp/hu_basic_directory_listing.hpp"

namespace legacy89diskkit::cpp
{
std::vector<HuBasicFileEntry> HuBasicShell::ListFiles(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size)
{
    return HuBasicDirectoryListing::ListFiles(directory_sectors, sector_size);
}

HuBasicDirectoryLayout HuBasicShell::ReadDirectoryLayout(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size)
{
    return HuBasicDirectoryLayoutParser::Parse(directory_sectors, sector_size);
}

HuBasicFileSystemInfo HuBasicShell::GetFileSystemInfo(
    const std::vector<std::uint8_t>& fat_data,
    const DiskType disk_type,
    const HuBasicConfiguration& config)
{
    return HuBasicFileSystemInfoRules::BuildInfo(fat_data, disk_type, config);
}

std::optional<HuBasicWriteTransactionPlan> HuBasicShell::PlanWrite(
    const char* file_name,
    const std::vector<std::uint8_t>& data,
    const HuBasicFileAttributes& attributes,
    const DiskType disk_type,
    const HuBasicConfiguration& config,
    const std::vector<std::uint8_t>& fat_data,
    const std::uint16_t load_address,
    const std::uint16_t execution_address)
{
    return HuBasicWriteTransaction::CreatePlan(
        file_name,
        data,
        attributes,
        disk_type,
        config,
        fat_data,
        load_address,
        execution_address);
}

std::optional<HuBasicDeleteTransactionPlan> HuBasicShell::PlanDelete(
    const std::vector<std::uint8_t>& fat_data,
    const std::vector<int>& clusters,
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* file_name)
{
    return HuBasicDeleteTransaction::CreatePlan(
        fat_data,
        clusters,
        directory_sectors,
        sector_size,
        file_name);
}

std::optional<HuBasicRenameTransactionPlan> HuBasicShell::PlanRename(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* old_name,
    const char* new_name)
{
    return HuBasicRenameTransaction::CreatePlan(
        directory_sectors,
        sector_size,
        old_name,
        new_name);
}

std::optional<HuBasicAttributeUpdateTransactionPlan> HuBasicShell::PlanAttributeUpdate(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* file_name,
    const HuBasicFileAttributes& attributes)
{
    return HuBasicAttributeUpdateTransaction::CreatePlan(
        directory_sectors,
        sector_size,
        file_name,
        attributes);
}
}
