#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicWriteRules
{
public:
    static std::vector<std::uint8_t> PrepareWritePayload(
        const std::vector<std::uint8_t>& data,
        const N88BasicFileAttributes& attributes);

    static int GetClustersNeeded(int payload_size, const N88BasicConfiguration& config);
    static int GetTerminalFlagForLength(int payload_size, const N88BasicConfiguration& config);
};
}
