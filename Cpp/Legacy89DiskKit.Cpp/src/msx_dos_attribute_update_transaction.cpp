#include "legacy89diskkit/cpp/msx_dos_attribute_update_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_attribute_update_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_dir_parser.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
std::optional<MsxDosAttributeUpdateTransactionPlan> MsxDosAttributeUpdateTransaction::CreatePlan(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const MsxDosConfiguration& config,
    const char* file_name,
    const MsxDosFileAttributes& attributes)
{
    for (std::size_t sector_index = 0; sector_index < directory_sectors.size(); ++sector_index)
    {
        for (auto offset = 0; offset < config.sector_size; offset += 32)
        {
            const auto marker = directory_sectors[sector_index][offset];
            if (marker == 0x00)
            {
                break;
            }

            if (marker == 0xe5)
            {
                continue;
            }

            std::array<std::uint8_t, 32> entry_bytes{};
            std::copy_n(directory_sectors[sector_index].begin() + offset, 32, entry_bytes.begin());
            const auto entry = MsxDosDirParser::ParseFileEntry(entry_bytes);
            if (HuBasicNameRules::BuildDisplayName(entry.file_name, entry.extension) == file_name)
            {
                return MsxDosAttributeUpdateTransactionPlan{
                    static_cast<int>(sector_index),
                    offset,
                    MsxDosAttributeUpdateRules::UpdateAttributes(entry, attributes) };
            }
        }
    }

    return std::nullopt;
}
}
