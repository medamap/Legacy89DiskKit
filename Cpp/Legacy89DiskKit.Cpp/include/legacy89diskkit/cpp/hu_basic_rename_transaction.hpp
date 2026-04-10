#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct HuBasicRenameTransactionPlan
{
    int sector_index;
    int entry_offset;
    HuBasicFileEntry updated_entry;
};

class HuBasicRenameTransaction
{
public:
    static std::optional<HuBasicRenameTransactionPlan> CreatePlan(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* old_name,
        const char* new_name);
};
}
