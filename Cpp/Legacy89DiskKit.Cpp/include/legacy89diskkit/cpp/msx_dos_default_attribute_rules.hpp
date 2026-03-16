#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

namespace legacy89diskkit::cpp
{
class MsxDosDefaultAttributeRules
{
public:
    static MsxDosFileAttributes CreateDefaultAttributes(bool is_directory);
};
}
