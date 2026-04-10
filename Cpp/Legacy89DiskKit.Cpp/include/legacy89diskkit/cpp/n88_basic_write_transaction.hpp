#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct N88BasicWriteTransactionPlan
{
    std::vector<std::uint8_t> payload;
    std::vector<int> allocated_clusters;
    int terminal_flag;
    N88BasicFileEntry file_entry;
};

class N88BasicWriteTransaction
{
public:
    static std::optional<N88BasicWriteTransactionPlan> CreatePlan(
        const std::string& file_name,
        const std::vector<std::uint8_t>& data,
        const N88BasicFileAttributes& attributes,
        const N88BasicConfiguration& config,
        const std::vector<std::uint8_t>& fat_data);
};
}
