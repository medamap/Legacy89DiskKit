#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <cstdint>

namespace legacy89diskkit::cpp
{
class N88BasicModeRules
{
public:
    static bool IsAscii(std::uint8_t attribute_byte);
    static bool IsBinary(std::uint8_t attribute_byte);
    static std::uint8_t BuildAttributeByte(const N88BasicFileAttributes& attributes);
};
}
