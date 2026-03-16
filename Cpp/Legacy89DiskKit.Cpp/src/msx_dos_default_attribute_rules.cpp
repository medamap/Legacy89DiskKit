#include "legacy89diskkit/cpp/msx_dos_default_attribute_rules.hpp"

#include "legacy89diskkit/cpp/msx_dos_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
MsxDosFileAttributes MsxDosDefaultAttributeRules::CreateDefaultAttributes(const bool is_directory)
{
    MsxDosFileAttributes attributes{
        !is_directory,
        0x00,
        false,
        false,
        false,
        is_directory,
        false };
    attributes.raw_attributes = MsxDosModeRules::BuildAttributeByte(attributes);
    return attributes;
}
}
