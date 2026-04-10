#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <array>
#include <cstdint>

namespace legacy89diskkit::cpp
{
class MsxDosFileEntryWriter
{
public:
    static std::array<std::uint8_t, 32> Write(const MsxDosFileEntry& entry);
};
}
