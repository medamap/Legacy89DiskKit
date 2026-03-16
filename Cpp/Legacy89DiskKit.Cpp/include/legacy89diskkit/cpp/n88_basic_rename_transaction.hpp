#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct N88BasicRenameTransactionPlan
{
    int sector_index;
    int entry_offset;
    N88BasicFileEntry updated_entry;
};

class N88BasicRenameTransaction
{
public:
    static std::optional<N88BasicRenameTransactionPlan> CreatePlan(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const N88BasicConfiguration& config,
        const char* old_name,
        const char* new_name);
};
}
