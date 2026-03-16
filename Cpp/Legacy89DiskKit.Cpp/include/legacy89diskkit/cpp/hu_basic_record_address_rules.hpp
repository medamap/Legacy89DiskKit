#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

namespace legacy89diskkit::cpp
{
struct HuBasicPhysicalAddress
{
    int cylinder;
    int head;
    int sector;
};

class HuBasicRecordAddressRules
{
public:
    static HuBasicPhysicalAddress GetPhysicalAddressFromRecord(int record_number, const HuBasicConfiguration& config);
};
}
