#include "legacy89diskkit/cpp/hu_basic_attribute_update_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_mode_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicFileEntry HuBasicAttributeUpdateRules::UpdateAttributes(
    const HuBasicFileEntry& entry,
    const HuBasicFileAttributes& attributes)
{
    auto updated = entry;
    updated.attributes = attributes;
    updated.metadata.file_type = attributes.is_ascii ? HuBasicFileType::Ascii : HuBasicFileType::Binary;
    updated.metadata.is_hidden = attributes.is_hidden;
    updated.metadata.is_write_protected = attributes.is_read_only;
    updated.metadata.is_directory = attributes.is_directory;
    updated.metadata.raw_mode_byte = HuBasicModeRules::BuildModeByte(attributes);
    return updated;
}
}
