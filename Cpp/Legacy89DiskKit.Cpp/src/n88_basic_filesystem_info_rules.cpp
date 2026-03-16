#include "legacy89diskkit/cpp/n88_basic_filesystem_info_rules.hpp"

#include "legacy89diskkit/cpp/n88_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
int N88BasicFileSystemInfoRules::CountFreeClusters(
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config)
{
    auto free_clusters = 0;
    for (auto cluster = config.reserved_clusters; cluster < config.total_clusters; ++cluster)
    {
        if (!N88BasicAllocationRules::IsAllocatableCluster(config, cluster))
        {
            continue;
        }

        if (N88BasicFatRules::GetEntry(fat_data, cluster) == 0x00)
        {
            ++free_clusters;
        }
    }

    return free_clusters;
}

N88BasicFileSystemInfo N88BasicFileSystemInfoRules::BuildInfo(
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config)
{
    const auto free_clusters = CountFreeClusters(fat_data, config);
    return N88BasicFileSystemInfo{
        static_cast<std::int64_t>(config.total_clusters) * config.cluster_size,
        static_cast<std::int64_t>(free_clusters) * config.cluster_size,
        config.cluster_size,
        config.reserved_clusters };
}
}
