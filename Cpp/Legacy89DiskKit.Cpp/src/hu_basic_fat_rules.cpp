#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"

#include <unordered_set>

namespace legacy89diskkit::cpp
{
int HuBasicFatRules::GetEntry(const std::vector<std::uint8_t>& fat_data, int cluster)
{
    if (cluster < 0 || cluster >= static_cast<int>(fat_data.size()))
    {
        return 0x8f;
    }

    return fat_data[cluster];
}

void HuBasicFatRules::SetEntry(std::vector<std::uint8_t>& fat_data, int cluster, int value)
{
    if (cluster >= 0 && cluster < static_cast<int>(fat_data.size()))
    {
        fat_data[cluster] = static_cast<std::uint8_t>(value);
    }
}

bool HuBasicFatRules::IsTerminal(int value)
{
    return (value >= 0x80 && value <= 0x8f) || value == 0xff;
}

int HuBasicFatRules::GetLastClusterUsedSectors(int terminal_flag)
{
    if (terminal_flag < 0x80 || terminal_flag > 0x8f)
    {
        return 0;
    }

    return terminal_flag - 0x7f;
}

HuBasicFatChainResult HuBasicFatRules::GetClusterChain(const std::vector<std::uint8_t>& fat_data, const HuBasicConfiguration& config, int start_cluster)
{
    std::vector<int> chain;
    std::unordered_set<int> visited;
    auto current = start_cluster;
    auto terminal_flag = 0xff;

    while (current >= config.reserved_clusters && current < config.total_clusters)
    {
        if (visited.contains(current))
        {
            break;
        }

        visited.insert(current);
        chain.push_back(current);

        const auto next = GetEntry(fat_data, current);
        if (IsTerminal(next))
        {
            terminal_flag = next;
            break;
        }

        current = next;
    }

    return HuBasicFatChainResult{ chain, terminal_flag };
}

void HuBasicFatRules::ApplyChain(std::vector<std::uint8_t>& fat_data, const std::vector<int>& clusters, int terminal_flag)
{
    for (std::size_t i = 0; i < clusters.size(); ++i)
    {
        const auto next = i + 1 == clusters.size() ? terminal_flag : clusters[i + 1];
        SetEntry(fat_data, clusters[i], next);
    }
}
}
