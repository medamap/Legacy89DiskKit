#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosWriteRules
{
public:
    static std::vector<std::uint8_t> PrepareWritePayload(
        const std::vector<std::uint8_t>& data,
        const MsxDosFileAttributes& attributes);

    static int GetClustersNeeded(int payload_size, const MsxDosConfiguration& config);
};
}
