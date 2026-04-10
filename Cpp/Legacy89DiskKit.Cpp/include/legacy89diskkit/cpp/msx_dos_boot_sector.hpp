#pragma once

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <array>
#include <cstdint>

namespace legacy89diskkit::cpp
{
struct MsxDosBootSector
{
    std::array<std::uint8_t, 3> jump;
    std::array<std::uint8_t, 8> oem_name;
    MsxDosConfiguration configuration;
};
}
