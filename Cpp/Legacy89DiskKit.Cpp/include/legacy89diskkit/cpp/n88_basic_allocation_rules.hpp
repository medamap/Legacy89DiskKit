#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicAllocationRules
{
public:
    static bool IsAllocatableCluster(const N88BasicConfiguration& config, int cluster);

    static std::vector<int> CollectFreeClusters(
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config,
        int count);
};
}
