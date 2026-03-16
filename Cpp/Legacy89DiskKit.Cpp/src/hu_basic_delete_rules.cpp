#include "legacy89diskkit/cpp/hu_basic_delete_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
void HuBasicDeleteRules::FreeClusters(std::vector<std::uint8_t>& fat_data, const std::vector<int>& clusters)
{
    for (const auto cluster : clusters)
    {
        HuBasicFatRules::SetEntry(fat_data, cluster, 0x00);
    }
}
}
