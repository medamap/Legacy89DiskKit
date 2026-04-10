#include "legacy89diskkit/cpp/n88_basic_allocation_rules.hpp"

#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
bool N88BasicAllocationRules::IsAllocatableCluster(const N88BasicConfiguration& config, const int cluster)
{
    return cluster >= config.reserved_clusters && cluster < config.total_clusters;
}

std::vector<int> N88BasicAllocationRules::CollectFreeClusters(
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config,
    const int count)
{
    std::vector<int> clusters;
    clusters.reserve(count);

    for (auto cluster = config.reserved_clusters;
         cluster < config.total_clusters && static_cast<int>(clusters.size()) < count;
         ++cluster)
    {
        if (N88BasicFatRules::GetEntry(fat_data, cluster) == 0x00)
        {
            clusters.push_back(cluster);
        }
    }

    return clusters;
}
}
