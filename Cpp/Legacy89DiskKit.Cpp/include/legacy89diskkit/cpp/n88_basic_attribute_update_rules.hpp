#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class N88BasicAttributeUpdateRules
{
public:
    static N88BasicFileEntry UpdateAttributes(
        const N88BasicFileEntry& entry,
        const N88BasicFileAttributes& attributes);
};
}
