#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class HuBasicAttributeUpdateRules
{
public:
    static HuBasicFileEntry UpdateAttributes(const HuBasicFileEntry& entry, const HuBasicFileAttributes& attributes);
};
}
