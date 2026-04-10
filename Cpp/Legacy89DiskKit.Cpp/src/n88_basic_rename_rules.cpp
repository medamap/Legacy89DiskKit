#include "legacy89diskkit/cpp/n88_basic_rename_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
N88BasicFileEntry N88BasicRenameRules::Rename(const N88BasicFileEntry& entry, const std::string& new_name)
{
    const auto parsed = HuBasicNameRules::ParseFileName(new_name);
    auto renamed = entry;
    renamed.file_name = parsed.file_name;
    renamed.extension = parsed.extension;
    return renamed;
}
}
