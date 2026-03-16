#pragma once

#include "legacy89diskkit/cpp/hu_basic_attribute_update_transaction.hpp"
#include "legacy89diskkit/cpp/hu_basic_delete_transaction.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_layout_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_rename_transaction.hpp"
#include "legacy89diskkit/cpp/hu_basic_write_transaction.hpp"

namespace legacy89diskkit::cpp
{
class HuBasicShell
{
public:
    static std::vector<HuBasicFileEntry> ListFiles(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size);

    static HuBasicDirectoryLayout ReadDirectoryLayout(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size);

    static HuBasicFileSystemInfo GetFileSystemInfo(
        const std::vector<std::uint8_t>& fat_data,
        DiskType disk_type,
        const HuBasicConfiguration& config);

    static std::optional<HuBasicWriteTransactionPlan> PlanWrite(
        const char* file_name,
        const std::vector<std::uint8_t>& data,
        const HuBasicFileAttributes& attributes,
        DiskType disk_type,
        const HuBasicConfiguration& config,
        const std::vector<std::uint8_t>& fat_data,
        std::uint16_t load_address,
        std::uint16_t execution_address);

    static std::optional<HuBasicDeleteTransactionPlan> PlanDelete(
        const std::vector<std::uint8_t>& fat_data,
        const std::vector<int>& clusters,
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* file_name);

    static std::optional<HuBasicRenameTransactionPlan> PlanRename(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* old_name,
        const char* new_name);

    static std::optional<HuBasicAttributeUpdateTransactionPlan> PlanAttributeUpdate(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* file_name,
        const HuBasicFileAttributes& attributes);
};
}
