#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"
#include "legacy89diskkit/cpp/msx_dos_attribute_update_transaction.hpp"
#include "legacy89diskkit/cpp/msx_dos_delete_transaction.hpp"
#include "legacy89diskkit/cpp/msx_dos_format_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_directory_listing.hpp"
#include "legacy89diskkit/cpp/msx_dos_file_lookup.hpp"
#include "legacy89diskkit/cpp/msx_dos_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_rename_transaction.hpp"
#include "legacy89diskkit/cpp/msx_dos_write_transaction.hpp"

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

    static std::optional<MsxDosWriteTransactionPlan> PlanWrite(
        const char* file_name,
        const std::vector<std::uint8_t>& data,
        const MsxDosFileAttributes& attributes,
        const MsxDosConfiguration& config,
        const std::vector<std::uint8_t>& fat_data);

    static std::optional<MsxDosDeleteTransactionPlan> PlanDelete(
        const std::vector<std::uint8_t>& fat_data,
        const std::vector<int>& clusters,
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const MsxDosConfiguration& config,
        const char* file_name);

    static std::optional<MsxDosRenameTransactionPlan> PlanRename(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const MsxDosConfiguration& config,
        const char* old_name,
        const char* new_name);

    static std::optional<MsxDosAttributeUpdateTransactionPlan> PlanAttributeUpdate(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const MsxDosConfiguration& config,
        const char* file_name,
        const MsxDosFileAttributes& attributes);

    static std::vector<std::uint8_t> CreateFatData(const MsxDosConfiguration& config);
    static std::vector<std::vector<std::uint8_t>> CreateRootDirectorySectors(const MsxDosConfiguration& config);
};
}
