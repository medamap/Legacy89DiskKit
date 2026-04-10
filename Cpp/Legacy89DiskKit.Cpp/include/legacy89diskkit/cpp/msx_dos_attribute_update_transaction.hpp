#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"
#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct MsxDosAttributeUpdateTransactionPlan
{
    int sector_index;
    int entry_offset;
    MsxDosFileEntry updated_entry;
};

class MsxDosAttributeUpdateTransaction
{
public:
    static std::optional<MsxDosAttributeUpdateTransactionPlan> CreatePlan(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const MsxDosConfiguration& config,
        const char* file_name,
        const MsxDosFileAttributes& attributes);
};
}
