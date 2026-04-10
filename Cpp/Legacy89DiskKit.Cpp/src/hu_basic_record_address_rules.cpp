#include "legacy89diskkit/cpp/hu_basic_record_address_rules.hpp"

namespace legacy89diskkit::cpp
{
HuBasicPhysicalAddress HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(
    const int record_number,
    const HuBasicConfiguration& config)
{
    return HuBasicPhysicalAddress
    {
        (record_number / config.sectors_per_track) / 2,
        (record_number / config.sectors_per_track) % 2,
        (record_number % config.sectors_per_track) + 1
    };
}
}
