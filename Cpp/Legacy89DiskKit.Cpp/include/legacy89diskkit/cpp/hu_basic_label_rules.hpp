#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class HuBasicLabelRules
{
public:
    static bool IsVirtualLabelEntry(const HuBasicFileEntry& entry);
    static bool CanMergeLabelEntries(const HuBasicFileEntry& previous, const HuBasicFileEntry& current);
};
}
