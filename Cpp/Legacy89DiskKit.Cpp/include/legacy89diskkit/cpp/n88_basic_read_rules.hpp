#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicReadRules
{
public:
    static std::uint32_t ResolveSizeFromFat(
        const std::vector<int>& clusters,
        const std::vector<std::uint8_t>& fat_data,
        const N88BasicConfiguration& config);

    static std::vector<std::uint8_t> ResolveReadPayload(
        const std::vector<std::uint8_t>& data,
        const N88BasicFileEntry& file);
};
}
