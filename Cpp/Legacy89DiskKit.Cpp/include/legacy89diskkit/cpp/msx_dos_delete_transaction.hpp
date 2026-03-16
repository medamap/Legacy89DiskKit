#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <cstdint>
#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
struct MsxDosDeleteTransactionPlan
{
    std::vector<std::uint8_t> fat_data;
    int sector_index;
    int entry_offset;
};

class MsxDosDeleteTransaction
{
public:
    static std::optional<MsxDosDeleteTransactionPlan> CreatePlan(
        const std::vector<std::uint8_t>& fat_data,
        const std::vector<int>& clusters,
        const std::vector<std::vector<std::uint8_t>>& directory_sectors,
        const MsxDosConfiguration& config,
        const char* file_name);
};
}
