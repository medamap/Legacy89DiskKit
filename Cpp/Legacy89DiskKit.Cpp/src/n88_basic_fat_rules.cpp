#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
std::uint8_t N88BasicFatRules::GetEntry(const std::vector<std::uint8_t>& fat_data, const int cluster)
{
    if (cluster < 0 || cluster >= static_cast<int>(fat_data.size()))
    {
        return 0xfe;
    }

    return fat_data[cluster];
}

void N88BasicFatRules::SetEntry(std::vector<std::uint8_t>& fat_data, const int cluster, const int value)
{
    if (cluster >= 0 && cluster < static_cast<int>(fat_data.size()))
    {
        fat_data[cluster] = static_cast<std::uint8_t>(value);
    }
}

bool N88BasicFatRules::IsEndOfChain(const std::uint8_t value)
{
    return value >= 0xc0 && value <= 0xcf;
}

int N88BasicFatRules::GetUsedSectorsInLastCluster(const std::uint8_t value)
{
    return IsEndOfChain(value) ? value - 0xc0 : 0;
}

std::vector<int> N88BasicFatRules::GetClusterChain(
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config,
    const int start_cluster)
{
    std::vector<int> chain;
    auto current = start_cluster;
    auto safety_limit = config.total_clusters;

    while (safety_limit-- > 0)
    {
        if (current == 0xff || current == 0xfe)
        {
            break;
        }

        chain.push_back(current);
        const auto entry = GetEntry(fat_data, current);
        if (IsEndOfChain(entry) || entry >= config.total_clusters)
        {
            break;
        }

        current = entry;
    }

    return chain;
}
}
