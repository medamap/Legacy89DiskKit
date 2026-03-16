#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct HuBasicAttributeUpdateTransactionPlan
{
    int sector_index;
    int entry_offset;
    HuBasicFileEntry updated_entry;
};

class HuBasicAttributeUpdateTransaction
{
public:
    static std::optional<HuBasicAttributeUpdateTransactionPlan> CreatePlan(
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* file_name,
        const HuBasicFileAttributes& attributes);
};
}
