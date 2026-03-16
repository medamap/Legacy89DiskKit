#include "legacy89diskkit/cpp/msx_dos_rename_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"

namespace legacy89diskkit::cpp
{
MsxDosFileEntry MsxDosRenameRules::Rename(const MsxDosFileEntry& entry, const std::string& new_name)
{
    const auto parsed = HuBasicNameRules::ParseFileName(new_name);
    auto renamed = entry;
    renamed.file_name = parsed.file_name;
    renamed.extension = parsed.extension;
    return renamed;
}
}
