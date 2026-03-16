#include "legacy89diskkit/cpp/n88_basic_default_attribute_rules.hpp"

namespace legacy89diskkit::cpp
{
N88BasicFileAttributes N88BasicDefaultAttributeRules::CreateDefaultAttributes(const bool is_ascii)
{
    return N88BasicFileAttributes
    {
        is_ascii,
        static_cast<std::uint8_t>(is_ascii ? 0x00 : 0x01),
        false
    };
}
}
