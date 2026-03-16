#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

#include <string>

namespace legacy89diskkit::cpp
{
class N88BasicRenameRules
{
public:
    static N88BasicFileEntry Rename(const N88BasicFileEntry& entry, const std::string& new_name);
};
}
