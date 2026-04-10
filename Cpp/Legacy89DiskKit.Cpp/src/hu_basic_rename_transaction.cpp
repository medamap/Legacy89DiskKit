#include "legacy89diskkit/cpp/hu_basic_rename_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_sector_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_rename_rules.hpp"

#include <array>
#include <algorithm>

namespace legacy89diskkit::cpp
{
std::optional<HuBasicRenameTransactionPlan> HuBasicRenameTransaction::CreatePlan(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* old_name,
    const char* new_name)
{
    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        const auto offset = HuBasicDirectorySectorRules::FindEntryOffset(
            directory_sectors[sector_index],
            sector_size,
            old_name);
        if (!offset.has_value())
        {
            continue;
        }

        std::array<std::uint8_t, 32> entry_bytes{};
        std::copy_n(directory_sectors[sector_index].begin() + *offset, 32, entry_bytes.begin());
        const auto entry = HuBasicDirParser::Parse(HuBasicDirectoryEntryCodec::Parse(entry_bytes));

        return HuBasicRenameTransactionPlan
        {
            static_cast<int>(sector_index),
            *offset,
            HuBasicRenameRules::Rename(entry, new_name)
        };
    }

    return std::nullopt;
}
}
