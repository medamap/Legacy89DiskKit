#include "legacy89diskkit/cpp/hu_basic_cluster_write_rules.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
std::vector<std::vector<std::uint8_t>> HuBasicClusterWriteRules::SplitIntoClusterBuffers(
    const std::vector<std::uint8_t>& data,
    const std::vector<int>& clusters,
    const HuBasicConfiguration& config)
{
    std::vector<std::vector<std::uint8_t>> buffers;
    buffers.reserve(clusters.size());

    auto offset = 0;
    for (std::size_t index = 0; index < clusters.size(); ++index)
    {
        std::vector<std::uint8_t> cluster_buffer(config.cluster_size, 0x00);
        const auto remaining = static_cast<int>(data.size()) - offset;
        const auto to_copy = std::max(0, std::min(remaining, config.cluster_size));
        if (to_copy > 0)
        {
            std::copy_n(data.begin() + offset, to_copy, cluster_buffer.begin());
            offset += to_copy;
        }

        buffers.push_back(std::move(cluster_buffer));
    }

    return buffers;
}
}
