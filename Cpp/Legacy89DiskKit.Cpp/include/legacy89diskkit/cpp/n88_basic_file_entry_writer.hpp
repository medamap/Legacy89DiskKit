#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <array>
#include <cstdint>

namespace legacy89diskkit::cpp
{
class N88BasicFileEntryWriter
{
public:
    static std::array<std::uint8_t, 16> Write(const N88BasicFileEntry& entry);
};
}
