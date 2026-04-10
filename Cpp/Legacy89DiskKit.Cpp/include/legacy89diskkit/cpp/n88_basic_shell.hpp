#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/n88_basic_attribute_update_transaction.hpp"
#include "legacy89diskkit/cpp/n88_basic_delete_transaction.hpp"
#include "legacy89diskkit/cpp/n88_basic_directory_listing.hpp"
#include "legacy89diskkit/cpp/n88_basic_file_lookup.hpp"
#include "legacy89diskkit/cpp/n88_basic_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_rename_transaction.hpp"
#include "legacy89diskkit/cpp/n88_basic_write_transaction.hpp"

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

    static std::optional<N88BasicFileEntry> FindFile(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config,
        const char* file_name);

    static N88BasicFileSystemInfo GetFileSystemInfo(
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config);

    static std::optional<N88BasicWriteTransactionPlan> PlanWrite(
        const char* file_name,
        const std::vector<std::uint8_t>& data,
        const N88BasicFileAttributes& attributes,
        const N88BasicConfiguration& config,
        const std::vector<std::uint8_t>& fat_data);

    static std::optional<N88BasicDeleteTransactionPlan> PlanDelete(
        const std::vector<std::uint8_t>& fat_data,
        const std::vector<int>& clusters,
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const N88BasicConfiguration& config,
        const char* file_name);

    static std::optional<N88BasicRenameTransactionPlan> PlanRename(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const N88BasicConfiguration& config,
        const char* old_name,
        const char* new_name);

    static std::optional<N88BasicAttributeUpdateTransactionPlan> PlanAttributeUpdate(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const N88BasicConfiguration& config,
        const char* file_name,
        const N88BasicFileAttributes& attributes);

    static std::vector<std::uint8_t> CreateFatData(const N88BasicConfiguration& config);
    static std::vector<std::vector<std::uint8_t>> CreateDirectorySectors(const N88BasicConfiguration& config);
};
}
