#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <cstdint>

namespace legacy89diskkit::cpp
{
class MsxDosModeRules
{
public:
    static MsxDosFileAttributes Parse(std::uint8_t attribute_byte);
    static std::uint8_t BuildAttributeByte(const MsxDosFileAttributes& attributes);
};
}
