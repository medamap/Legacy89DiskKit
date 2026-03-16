#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <string>

namespace legacy89diskkit::cpp
{
class HuBasicRenameRules
{
public:
    static HuBasicFileEntry Rename(const HuBasicFileEntry& entry, const std::string& new_name);
};
}
