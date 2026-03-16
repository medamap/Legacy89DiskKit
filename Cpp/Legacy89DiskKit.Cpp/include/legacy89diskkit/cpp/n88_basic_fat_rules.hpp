#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicFatRules
{
public:
    static std::uint8_t GetEntry(const std::vector<std::uint8_t>& fat_data, int cluster);
    static void SetEntry(std::vector<std::uint8_t>& fat_data, int cluster, int value);
    static bool IsEndOfChain(std::uint8_t value);
    static int GetUsedSectorsInLastCluster(std::uint8_t value);
    static std::vector<int> GetClusterChain(
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config,
        int start_cluster);
};
}
