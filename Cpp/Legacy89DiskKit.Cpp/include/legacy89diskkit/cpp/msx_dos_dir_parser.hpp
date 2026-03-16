#pragma once

#include "legacy89diskkit/cpp/msx_dos_directory_entry.hpp"
#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <array>

namespace legacy89diskkit::cpp
{
class MsxDosDirParser
{
public:
    static MsxDosDirectoryEntry ParseEntry(const std::array<std::uint8_t, 32>& entry_data);
    static MsxDosFileEntry ParseFileEntry(const std::array<std::uint8_t, 32>& entry_data);
    static std::array<std::uint8_t, 32> Write(const MsxDosFileEntry& entry);
};
}
