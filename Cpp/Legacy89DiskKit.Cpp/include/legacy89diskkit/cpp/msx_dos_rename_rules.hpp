#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <string>

namespace legacy89diskkit::cpp
{
class MsxDosRenameRules
{
public:
    static MsxDosFileEntry Rename(const MsxDosFileEntry& entry, const std::string& new_name);
};
}
