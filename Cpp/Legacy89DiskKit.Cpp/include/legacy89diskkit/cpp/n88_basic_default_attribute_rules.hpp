#pragma once

#include "legacy89diskkit/cpp/n88_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class N88BasicDefaultAttributeRules
{
public:
    static N88BasicFileAttributes CreateDefaultAttributes(bool is_ascii);
};
}
