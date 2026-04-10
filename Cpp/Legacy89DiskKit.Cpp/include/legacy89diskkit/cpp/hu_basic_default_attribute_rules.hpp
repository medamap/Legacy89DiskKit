#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class HuBasicDefaultAttributeRules
{
public:
    static HuBasicFileAttributes CreateDefaultAttributes(bool is_ascii);
};
}
