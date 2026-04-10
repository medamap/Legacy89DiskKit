#include "legacy89diskkit/cpp/n88_basic_format_rules.hpp"

#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<std::uint8_t> N88BasicFormatRules::CreateFatData(const N88BasicConfiguration& config)
{
    std::vector<std::uint8_t> fat_data(config.fat_sectors * config.sector_size, 0x00);
    for (auto cluster = 0; cluster < config.reserved_clusters; ++cluster)
    {
        N88BasicFatRules::SetEntry(fat_data, cluster, 0xff);
    }

    return fat_data;
}

std::vector<std::vector<std::uint8_t>> N88BasicFormatRules::CreateDirectorySectors(const N88BasicConfiguration& config)
{
    return std::vector<std::vector<std::uint8_t>>(
        config.directory_sectors,
        std::vector<std::uint8_t>(config.sector_size, 0xff));
}
}
