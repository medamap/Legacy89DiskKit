#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosClusterWriteRules
{
public:
    static std::vector<std::vector<std::uint8_t>> SplitIntoClusterBuffers(
        const std::vector<std::uint8_t>& data,
        const std::vector<int>& clusters,
        const MsxDosConfiguration& config);
};
}
