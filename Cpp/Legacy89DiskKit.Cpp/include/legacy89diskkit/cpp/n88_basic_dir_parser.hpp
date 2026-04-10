#pragma once

#include "legacy89diskkit/cpp/n88_basic_directory_entry.hpp"
#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <array>

namespace legacy89diskkit::cpp
{
class N88BasicDirParser
{
public:
    static N88BasicDirectoryEntry ParseEntry(const std::array<std::uint8_t, 16>& entry_data);
    static N88BasicFileEntry ParseFileEntry(const std::array<std::uint8_t, 16>& entry_data);
    static std::array<std::uint8_t, 16> Write(const N88BasicFileEntry& entry);
};
}
