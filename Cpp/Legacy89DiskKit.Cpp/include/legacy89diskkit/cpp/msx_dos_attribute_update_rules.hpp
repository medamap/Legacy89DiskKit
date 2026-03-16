#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

namespace legacy89diskkit::cpp
{
class MsxDosAttributeUpdateRules
{
public:
    static MsxDosFileEntry UpdateAttributes(
        const MsxDosFileEntry& entry,
        const MsxDosFileAttributes& attributes);
};
}
