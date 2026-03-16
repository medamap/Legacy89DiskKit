#include "legacy89diskkit/cpp/hu_basic_allocation_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
int HuBasicAllocationRules::GetFatScanLimit(DiskType disk_type, const HuBasicConfiguration& config)
{
    return disk_type == DiskType::TwoHD ? 512 : config.total_clusters;
}

bool HuBasicAllocationRules::IsAllocatableCluster(DiskType disk_type, const HuBasicConfiguration& config, int cluster)
{
    if (cluster < config.reserved_clusters)
    {
        return false;
    }

    if (disk_type == DiskType::TwoHD && (cluster % 256) >= 0x80)
    {
        return false;
    }

    return cluster < GetFatScanLimit(disk_type, config);
}

std::vector<int> HuBasicAllocationRules::CollectFreeClusters(
    const std::vector<std::uint8_t>& fat_data,
    DiskType disk_type,
    const HuBasicConfiguration& config,
    int count)
{
    std::vector<int> allocated;
    const auto max_index = GetFatScanLimit(disk_type, config);

    for (auto cluster = config.reserved_clusters; cluster < max_index && static_cast<int>(allocated.size()) < count; ++cluster)
    {
        if (!IsAllocatableCluster(disk_type, config, cluster))
        {
            continue;
        }

        if (HuBasicFatRules::GetEntry(fat_data, cluster) == 0)
        {
            allocated.push_back(cluster);
        }
    }

    return allocated;
}
}
