#include "legacy89diskkit/cpp/n88_basic_delete_rules.hpp"

#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
void N88BasicDeleteRules::FreeClusters(std::vector<std::uint8_t>& fat_data, const std::vector<int>& clusters)
{
    for (const auto cluster : clusters)
    {
        N88BasicFatRules::SetEntry(fat_data, cluster, 0x00);
    }
}
}
