#include "legacy89diskkit/cpp/n88_basic_attribute_update_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_attribute_update_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_dir_parser.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
std::optional<N88BasicAttributeUpdateTransactionPlan> N88BasicAttributeUpdateTransaction::CreatePlan(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const N88BasicConfiguration& config,
    const char* file_name,
    const N88BasicFileAttributes& attributes)
{
    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        for (auto offset = 0; offset < config.sector_size; offset += 16)
        {
            const auto marker = directory_sectors[sector_index][offset];
            if (marker == 0xff)
            {
                break;
            }

            if (marker == 0x00)
            {
                continue;
            }

            std::array<std::uint8_t, 16> entry_bytes{};
            std::copy_n(directory_sectors[sector_index].begin() + offset, 16, entry_bytes.begin());
            const auto entry = N88BasicDirParser::ParseFileEntry(entry_bytes);
            if (HuBasicNameRules::BuildDisplayName(entry.file_name, entry.extension) == file_name)
            {
                return N88BasicAttributeUpdateTransactionPlan{
                    static_cast<int>(sector_index),
                    offset,
                    N88BasicAttributeUpdateRules::UpdateAttributes(entry, attributes) };
            }
        }
    }

    return std::nullopt;
}
}
