#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicClusterWriteRules
{
public:
    static std::vector<std::vector<std::uint8_t>> SplitIntoClusterBuffers(
        const std::vector<std::uint8_t>& data,
        const std::vector<int>& clusters,
        const HuBasicConfiguration& config);
};
}
