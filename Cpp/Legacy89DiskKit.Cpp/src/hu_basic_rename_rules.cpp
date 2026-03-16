#include "legacy89diskkit/cpp/hu_basic_rename_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicFileEntry HuBasicRenameRules::Rename(const HuBasicFileEntry& entry, const std::string& new_name)
{
    const auto parsed_name = HuBasicNameRules::ParseFileName(new_name);
    auto renamed = entry;
    renamed.file_name = parsed_name.file_name;
    renamed.extension = parsed_name.extension;
    return renamed;
}
}
