#include "legacy89diskkit/cpp/hu_basic_delete_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_delete_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_sector_rules.hpp"

#include <string>

namespace legacy89diskkit::cpp
{
std::optional<HuBasicDeleteTransactionPlan> HuBasicDeleteTransaction::CreatePlan(
    const std::vector<std::uint8_t>& fat_data,
    const std::vector<int>& clusters,
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* file_name)
{
    auto updated_fat = fat_data;
    HuBasicDeleteRules::FreeClusters(updated_fat, clusters);

    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        const auto offset = HuBasicDirectorySectorRules::FindEntryOffset(
            directory_sectors[sector_index],
            sector_size,
            file_name);
        if (offset.has_value())
        {
            return HuBasicDeleteTransactionPlan
            {
                updated_fat,
                static_cast<int>(sector_index),
                *offset
            };
        }
    }

    return std::nullopt;
}
}
