#include "legacy89diskkit/cpp/n88_basic_default_attribute_rules.hpp"

#include "legacy89diskkit/cpp/n88_basic_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
N88BasicFileAttributes N88BasicDefaultAttributeRules::CreateDefaultAttributes(const bool is_ascii)
{
    return N88BasicFileAttributes
    {
        is_ascii,
        N88BasicModeRules::BuildAttributeByte(N88BasicFileAttributes{ is_ascii, 0x00, false }),
        false
    };
}
}
