#include "legacy89diskkit/cpp/msx_dos_filesystem_info_rules.hpp"

#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
int MsxDosFileSystemInfoRules::CountFreeClusters(
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config)
{
    auto free_clusters = 0;
    for (auto cluster = 2; cluster < config.TotalClusters() + 2; ++cluster)
    {
        if (MsxDosFatRules::GetEntry(fat_data, cluster) == 0x000)
        {
            ++free_clusters;
        }
    }

    return free_clusters;
}

MsxDosFileSystemInfo MsxDosFileSystemInfoRules::BuildInfo(
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config)
{
    const auto free_clusters = CountFreeClusters(fat_data, config);
    return MsxDosFileSystemInfo{
        static_cast<std::int64_t>(config.TotalClusters()) * config.ClusterSize(),
        static_cast<std::int64_t>(free_clusters) * config.ClusterSize(),
        config.ClusterSize(),
        config.FirstDataSector() };
}
}
