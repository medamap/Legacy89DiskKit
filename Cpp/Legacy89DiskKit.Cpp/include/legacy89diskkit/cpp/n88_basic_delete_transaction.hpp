#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct N88BasicDeleteTransactionPlan
{
    std::vector<std::uint8_t> fat_data;
    int sector_index;
    int entry_offset;
};

class N88BasicDeleteTransaction
{
public:
    static std::optional<N88BasicDeleteTransactionPlan> CreatePlan(
        const std::vector<std::uint8_t>& fat_data,
        const std::vector<int>& clusters,
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const N88BasicConfiguration& config,
        const char* file_name);
};
}
