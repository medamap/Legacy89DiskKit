#include "legacy89diskkit/cpp/msx_dos_attribute_update_rules.hpp"

#include "legacy89diskkit/cpp/msx_dos_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
MsxDosFileEntry MsxDosAttributeUpdateRules::UpdateAttributes(
    const MsxDosFileEntry& entry,
    const MsxDosFileAttributes& attributes)
{
    auto updated = entry;
    updated.attributes = attributes;
    updated.attributes.raw_attributes = MsxDosModeRules::BuildAttributeByte(attributes);
    return updated;
}
}
