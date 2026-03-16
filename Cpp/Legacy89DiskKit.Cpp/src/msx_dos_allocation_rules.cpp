#include "legacy89diskkit/cpp/msx_dos_allocation_rules.hpp"

#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<int> MsxDosAllocationRules::CollectFreeClusters(
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config,
    const int count)
{
    std::vector<int> clusters;
    clusters.reserve(count);

    for (auto cluster = 2;
         cluster < config.TotalClusters() + 2 && static_cast<int>(clusters.size()) < count;
         ++cluster)
    {
        if (MsxDosFatRules::GetEntry(fat_data, cluster) == 0x000)
        {
            clusters.push_back(cluster);
        }
    }

    return clusters;
}
}
