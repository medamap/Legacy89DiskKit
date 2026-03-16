#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct N88BasicAttributeUpdateTransactionPlan
{
    int sector_index;
    int entry_offset;
    N88BasicFileEntry updated_entry;
};

class N88BasicAttributeUpdateTransaction
{
public:
    static std::optional<N88BasicAttributeUpdateTransactionPlan> CreatePlan(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const N88BasicConfiguration& config,
        const char* file_name,
        const N88BasicFileAttributes& attributes);
};
}
