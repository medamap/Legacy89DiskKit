#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"
#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosReadRules
{
public:
    static std::uint32_t ResolveSizeFromFat(
        const std::vector<int>& clusters,
        const MsxDosConfiguration& config,
        std::uint32_t declared_size);

    static std::vector<std::uint8_t> ResolveReadPayload(
        const std::vector<std::uint8_t>& data,
        const MsxDosFileEntry& file);
};
}
