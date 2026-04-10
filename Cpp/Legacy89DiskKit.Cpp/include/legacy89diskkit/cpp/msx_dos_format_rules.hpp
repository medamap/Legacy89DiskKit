#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosFormatRules
{
public:
    static std::vector<std::uint8_t> CreateFatData(const MsxDosConfiguration& config);
    static std::vector<std::vector<std::uint8_t>> CreateRootDirectorySectors(const MsxDosConfiguration& config);
};
}
