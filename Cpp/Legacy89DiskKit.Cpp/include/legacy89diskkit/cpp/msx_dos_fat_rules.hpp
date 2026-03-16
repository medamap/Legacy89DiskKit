#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosFatRules
{
public:
    static std::uint16_t GetEntry(const std::vector<std::uint8_t>& fat_data, int cluster);
    static void SetEntry(std::vector<std::uint8_t>& fat_data, int cluster, std::uint16_t value);
    static bool IsEndOfChain(std::uint16_t value);
    static std::vector<int> GetClusterChain(
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config,
        int start_cluster);
};
}
