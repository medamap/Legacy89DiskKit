#include "legacy89diskkit/cpp/hu_basic_filesystem_info_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
int HuBasicFileSystemInfoRules::CountFreeClusters(
    const std::vector<std::uint8_t>& fat_data,
    const DiskType disk_type,
    const HuBasicConfiguration& config)
{
    auto free_clusters = 0;
    const auto limit = HuBasicAllocationRules::GetFatScanLimit(disk_type, config);
    for (auto cluster = config.reserved_clusters; cluster < limit; ++cluster)
    {
        if (!HuBasicAllocationRules::IsAllocatableCluster(disk_type, config, cluster))
        {
            continue;
        }

        if (HuBasicFatRules::GetEntry(fat_data, cluster) == 0x00)
        {
            ++free_clusters;
        }
    }

    return free_clusters;
}

HuBasicFileSystemInfo HuBasicFileSystemInfoRules::BuildInfo(
    const std::vector<std::uint8_t>& fat_data,
    const DiskType disk_type,
    const HuBasicConfiguration& config)
{
    const auto free_clusters = CountFreeClusters(fat_data, disk_type, config);
    return HuBasicFileSystemInfo
    {
        static_cast<std::int64_t>(config.total_clusters) * config.cluster_size,
        static_cast<std::int64_t>(free_clusters) * config.cluster_size,
        config.cluster_size,
        config.reserved_clusters * (config.cluster_size / config.sector_size)
    };
}
}
