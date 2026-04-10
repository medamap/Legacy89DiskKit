#pragma once

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicDeleteRules
{
public:
    static void FreeClusters(std::vector<std::uint8_t>& fat_data, const std::vector<int>& clusters);
};
}
