#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicAllocationRules
{
public:
    static int GetFatScanLimit(DiskType disk_type, const HuBasicConfiguration& config);
    static bool IsAllocatableCluster(DiskType disk_type, const HuBasicConfiguration& config, int cluster);
    static std::vector<int> CollectFreeClusters(
        const std::vector<std::uint8_t>& fat_data,
        DiskType disk_type,
        const HuBasicConfiguration& config,
        int count);
};
}
