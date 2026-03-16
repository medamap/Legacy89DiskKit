#include "legacy89diskkit/cpp/msx_dos_rename_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_dir_parser.hpp"
#include "legacy89diskkit/cpp/msx_dos_rename_rules.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
std::optional<MsxDosRenameTransactionPlan> MsxDosRenameTransaction::CreatePlan(
    const std::vector<std::vector<std::uint8_t>>& directory_sectors,
    const MsxDosConfiguration& config,
    const char* old_name,
    const char* new_name)
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
            if (HuBasicNameRules::BuildDisplayName(entry.file_name, entry.extension) == old_name)
            {
                return MsxDosRenameTransactionPlan{
                    static_cast<int>(sector_index),
                    offset,
                    MsxDosRenameRules::Rename(entry, new_name) };
            }
        }
    }

    return std::nullopt;
}
}
