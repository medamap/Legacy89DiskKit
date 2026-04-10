#include "legacy89diskkit/cpp/hu_basic_write_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<std::uint8_t> HuBasicWriteRules::PrepareWritePayload(const std::vector<std::uint8_t>& data, const HuBasicFileAttributes& attributes)
{
    if (!attributes.is_ascii)
    {
        return data;
    }

    if (!data.empty() && data.back() == 0x1a)
    {
        return data;
    }

    auto prepared = data;
    prepared.push_back(0x1a);
    return prepared;
}

int HuBasicWriteRules::GetClustersNeeded(int data_length, const HuBasicConfiguration& config)
{
    const auto clusters_needed = (data_length + config.cluster_size - 1) / config.cluster_size;
    return clusters_needed == 0 ? 1 : clusters_needed;
}

int HuBasicWriteRules::GetSectorsInLastCluster(int data_length, const HuBasicConfiguration& config)
{
    const auto sectors_per_cluster = config.cluster_size / config.sector_size;
    const auto sectors_in_last_cluster = ((data_length + config.sector_size - 1) / config.sector_size) % sectors_per_cluster;
    return sectors_in_last_cluster == 0 ? sectors_per_cluster : sectors_in_last_cluster;
}

int HuBasicWriteRules::GetTerminalFlagForLength(int data_length, const HuBasicConfiguration& config)
{
    return 0x7f + GetSectorsInLastCluster(data_length, config);
}
}
