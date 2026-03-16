#include "legacy89diskkit/cpp/hu_basic_default_attribute_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicFileAttributes HuBasicDefaultAttributeRules::CreateDefaultAttributes(const bool is_ascii)
{
    return HuBasicFileAttributes
    {
        is_ascii,
        static_cast<std::uint8_t>(is_ascii ? 0x04 : 0x01),
        false,
        false,
        false
    };
}
}
