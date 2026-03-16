#include "legacy89diskkit/cpp/hu_basic_format_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
std::vector<std::uint8_t> HuBasicFormatRules::CreateFatData(const HuBasicConfiguration& config)
{
    std::vector<std::uint8_t> fat_data(config.fat_sectors * config.sector_size, 0x00);
    for (auto cluster = 0; cluster < config.reserved_clusters; ++cluster)
    {
        const auto next = cluster == config.reserved_clusters - 1 ? 0x8f : cluster + 1;
        HuBasicFatRules::SetEntry(fat_data, cluster, next);
    }

    return fat_data;
}

std::vector<std::vector<std::uint8_t>> HuBasicFormatRules::CreateDirectorySectors(const HuBasicConfiguration& config)
{
    std::vector<std::vector<std::uint8_t>> sectors(
        config.directory_sectors,
        std::vector<std::uint8_t>(config.sector_size, 0xff));
    return sectors;
}
}
