#include "legacy89diskkit/cpp/n88_basic_write_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<std::uint8_t> N88BasicWriteRules::PrepareWritePayload(
    const std::vector<std::uint8_t>& data,
    const N88BasicFileAttributes& attributes)
{
    auto payload = data;
    if (attributes.is_ascii)
    {
        payload.push_back(0x1a);
    }

    return payload;
}

int N88BasicWriteRules::GetClustersNeeded(const int payload_size, const N88BasicConfiguration& config)
{
    if (payload_size <= 0)
    {
        return 1;
    }

    return (payload_size + config.cluster_size - 1) / config.cluster_size;
}

int N88BasicWriteRules::GetTerminalFlagForLength(const int payload_size, const N88BasicConfiguration& config)
{
    auto sectors_used = payload_size / config.sector_size;
    if ((payload_size % config.sector_size) != 0)
    {
        ++sectors_used;
    }

    const auto sectors_per_cluster = config.cluster_size / config.sector_size;
    const auto used_in_last_cluster = sectors_used == 0 ? 1 : ((sectors_used - 1) % sectors_per_cluster) + 1;
    return 0xc0 + used_in_last_cluster;
}
}
