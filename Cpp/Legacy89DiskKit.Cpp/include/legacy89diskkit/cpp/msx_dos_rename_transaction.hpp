#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"
#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct MsxDosRenameTransactionPlan
{
    int sector_index;
    int entry_offset;
    MsxDosFileEntry updated_entry;
};

class MsxDosRenameTransaction
{
public:
    static std::optional<MsxDosRenameTransactionPlan> CreatePlan(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const MsxDosConfiguration& config,
        const char* old_name,
        const char* new_name);
};
}
