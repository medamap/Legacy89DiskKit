#include "legacy89diskkit/cpp/n88_basic_attribute_update_rules.hpp"

namespace legacy89diskkit::cpp
{
N88BasicFileEntry N88BasicAttributeUpdateRules::UpdateAttributes(
    const N88BasicFileEntry& entry,
    const N88BasicFileAttributes& attributes)
{
    auto updated = entry;
    updated.attributes = attributes;
    return updated;
}
}
