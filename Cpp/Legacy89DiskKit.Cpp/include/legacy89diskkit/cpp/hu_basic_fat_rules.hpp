#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
struct HuBasicFatChainResult
{
    std::vector<int> chain;
    int terminal_flag;
};

class HuBasicFatRules
{
public:
    static int GetEntry(const std::vector<std::uint8_t>& fat_data, int cluster);
    static void SetEntry(std::vector<std::uint8_t>& fat_data, int cluster, int value);
    static bool IsTerminal(int value);
    static int GetLastClusterUsedSectors(int terminal_flag);
    static HuBasicFatChainResult GetClusterChain(const std::vector<std::uint8_t>& fat_data, const HuBasicConfiguration& config, int start_cluster);
    static void ApplyChain(std::vector<std::uint8_t>& fat_data, const std::vector<int>& clusters, int terminal_flag);
};
}
