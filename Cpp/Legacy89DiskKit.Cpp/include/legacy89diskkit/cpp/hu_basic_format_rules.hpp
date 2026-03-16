#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicFormatRules
{
public:
    static std::vector<std::uint8_t> CreateFatData(const HuBasicConfiguration& config);
    static std::vector<std::vector<std::uint8_t>> CreateDirectorySectors(const HuBasicConfiguration& config);
};
}
