#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosAllocationRules
{
public:
    static std::vector<int> CollectFreeClusters(
        const std::vector<std::uint8_t>& fat_data,
        const MsxDosConfiguration& config,
        int count);
};
}
