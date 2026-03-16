#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicWriteRules
{
public:
    static std::vector<std::uint8_t> PrepareWritePayload(const std::vector<std::uint8_t>& data, const HuBasicFileAttributes& attributes);
    static int GetClustersNeeded(int data_length, const HuBasicConfiguration& config);
    static int GetSectorsInLastCluster(int data_length, const HuBasicConfiguration& config);
    static int GetTerminalFlagForLength(int data_length, const HuBasicConfiguration& config);
};
}
