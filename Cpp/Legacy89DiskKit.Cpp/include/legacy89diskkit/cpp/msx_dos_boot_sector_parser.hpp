#pragma once

#include "legacy89diskkit/cpp/msx_dos_boot_sector.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosBootSectorParser
{
public:
    static std::optional<MsxDosBootSector> Parse(const std::vector<std::uint8_t>& sector_data);
    static std::vector<std::uint8_t> Write(const MsxDosBootSector& boot_sector);
};
}
