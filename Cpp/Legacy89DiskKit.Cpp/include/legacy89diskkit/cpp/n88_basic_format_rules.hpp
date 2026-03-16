#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicFormatRules
{
public:
    static std::vector<std::uint8_t> CreateFatData(const N88BasicConfiguration& config);
    static std::vector<std::vector<std::uint8_t>> CreateDirectorySectors(const N88BasicConfiguration& config);
};
}
