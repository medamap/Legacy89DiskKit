#pragma once

#include <cstdint>
#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct HuBasicDeleteTransactionPlan
{
    std::vector<std::uint8_t> fat_data;
    int sector_index;
    int entry_offset;
};

class HuBasicDeleteTransaction
{
public:
    static std::optional<HuBasicDeleteTransactionPlan> CreatePlan(
        const std::vector<std::uint8_t>& fat_data,
        const std::vector<int>& clusters,
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        int sector_size,
        const char* file_name);
};
}
