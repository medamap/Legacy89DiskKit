#include "legacy89diskkit/cpp/hu_basic_attribute_update_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_attribute_update_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_sector_rules.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
std::optional<HuBasicAttributeUpdateTransactionPlan> HuBasicAttributeUpdateTransaction::CreatePlan(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const int sector_size,
    const char* file_name,
    const HuBasicFileAttributes& attributes)
{
    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        const auto offset = HuBasicDirectorySectorRules::FindEntryOffset(
            directory_sectors[sector_index],
            sector_size,
            file_name);
        if (!offset.has_value())
        {
            continue;
        }

        std::array<std::uint8_t, 32> entry_bytes{};
        std::copy_n(directory_sectors[sector_index].begin() + *offset, 32, entry_bytes.begin());
        const auto entry = HuBasicDirParser::Parse(HuBasicDirectoryEntryCodec::Parse(entry_bytes));

        return HuBasicAttributeUpdateTransactionPlan
        {
            static_cast<int>(sector_index),
            *offset,
            HuBasicAttributeUpdateRules::UpdateAttributes(entry, attributes)
        };
    }

    return std::nullopt;
}
}
