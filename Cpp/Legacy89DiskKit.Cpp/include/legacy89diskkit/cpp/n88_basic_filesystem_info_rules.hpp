#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicFileSystemInfoRules
{
public:
    static int CountFreeClusters(
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config);

    static N88BasicFileSystemInfo BuildInfo(
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config);
};
}
