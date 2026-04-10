#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
std::uint16_t MsxDosFatRules::GetEntry(const std::vector<std::uint8_t>& fat_data, const int cluster)
{
    const auto offset = (cluster * 3) / 2;
    if (offset + 1 >= static_cast<int>(fat_data.size()))
    {
        return 0xfff;
    }

    if ((cluster % 2) == 0)
    {
        return static_cast<std::uint16_t>(((fat_data[offset + 1] & 0x0f) << 8) | fat_data[offset]);
    }

    return static_cast<std::uint16_t>((fat_data[offset + 1] << 4) | ((fat_data[offset] & 0xf0) >> 4));
}

void MsxDosFatRules::SetEntry(std::vector<std::uint8_t>& fat_data, const int cluster, const std::uint16_t value)
{
    const auto offset = (cluster * 3) / 2;
    if (offset + 1 >= static_cast<int>(fat_data.size()))
    {
        return;
    }

    if ((cluster % 2) == 0)
    {
        fat_data[offset] = static_cast<std::uint8_t>(value & 0xff);
        fat_data[offset + 1] = static_cast<std::uint8_t>((fat_data[offset + 1] & 0xf0) | ((value >> 8) & 0x0f));
    }
    else
    {
        fat_data[offset] = static_cast<std::uint8_t>((fat_data[offset] & 0x0f) | ((value << 4) & 0xf0));
        fat_data[offset + 1] = static_cast<std::uint8_t>((value >> 4) & 0xff);
    }
}

bool MsxDosFatRules::IsEndOfChain(const std::uint16_t value)
{
    return value >= 0xff8;
}

std::vector<int> MsxDosFatRules::GetClusterChain(
    const std::vector<std::uint8_t>& fat_data,
    const MsxDosConfiguration& config,
    const int start_cluster)
{
    std::vector<int> chain;
    if (start_cluster < 2 || start_cluster > 0xfef)
    {
        return chain;
    }

    auto current = start_cluster;
    auto safety_limit = config.TotalClusters() + 2;
    while (current >= 0x002 && current <= 0xfef && safety_limit-- > 0)
    {
        chain.push_back(current);
        const auto next = GetEntry(fat_data, current);
        if (IsEndOfChain(next))
        {
            break;
        }

        current = next;
    }

    return chain;
}
}
