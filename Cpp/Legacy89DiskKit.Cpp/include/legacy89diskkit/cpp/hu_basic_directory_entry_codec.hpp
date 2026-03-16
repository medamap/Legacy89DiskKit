#pragma once

#include "legacy89diskkit/cpp/hu_basic_directory_entry.hpp"

#include <array>
#include <cstdint>

namespace legacy89diskkit::cpp
{
class HuBasicDirectoryEntryCodec
{
public:
    static HuBasicDirectoryEntry Parse(const std::array<std::uint8_t, 32>& data);
    static std::array<std::uint8_t, 32> Write(const HuBasicDirectoryEntry& entry);
};
}
