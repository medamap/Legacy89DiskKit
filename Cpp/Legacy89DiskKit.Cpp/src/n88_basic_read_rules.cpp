#include "legacy89diskkit/cpp/n88_basic_read_rules.hpp"

#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
std::uint32_t N88BasicReadRules::ResolveSizeFromFat(
    const std::vector<int>& clusters,
    const std::vector<std::uint8_t>& fat_data,
    const N88BasicConfiguration& config)
{
    auto size = static_cast<std::uint32_t>(clusters.size() * config.cluster_size);
    if (!clusters.empty())
    {
        const auto last_entry = N88BasicFatRules::GetEntry(fat_data, clusters.back());
        if (N88BasicFatRules::IsEndOfChain(last_entry))
        {
            size = static_cast<std::uint32_t>((clusters.size() - 1) * config.cluster_size +
                                              (N88BasicFatRules::GetUsedSectorsInLastCluster(last_entry) * config.sector_size));
        }
    }

    return size;
}

std::vector<std::uint8_t> N88BasicReadRules::ResolveReadPayload(
    const std::vector<std::uint8_t>& data,
    const N88BasicFileEntry& file)
{
    auto payload = data;
    if (payload.size() > file.size)
    {
        payload.resize(file.size);
    }

    if (file.attributes.is_ascii)
    {
        const auto eof = std::find(payload.begin(), payload.end(), static_cast<std::uint8_t>(0x1a));
        if (eof != payload.end())
        {
            payload.erase(eof, payload.end());
        }
    }

    return payload;
}
}
