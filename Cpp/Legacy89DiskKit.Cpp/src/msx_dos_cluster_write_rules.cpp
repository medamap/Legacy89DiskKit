#include "legacy89diskkit/cpp/msx_dos_cluster_write_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<std::vector<std::uint8_t>> MsxDosClusterWriteRules::SplitIntoClusterBuffers(
    const std::vector<std::uint8_t>& data,
    const std::vector<int>& clusters,
    const MsxDosConfiguration& config)
{
    std::vector<std::vector<std::uint8_t>> buffers;
    buffers.reserve(clusters.size());

    for (std::size_t index = 0; index < clusters.size(); ++index)
    {
        std::vector<std::uint8_t> buffer(config.ClusterSize(), 0x00);
        const auto offset = static_cast<int>(index) * config.ClusterSize();
        const auto remaining = offset < static_cast<int>(data.size()) ? static_cast<int>(data.size()) - offset : 0;
        const auto count = remaining < config.ClusterSize() ? remaining : config.ClusterSize();
        for (auto byte_index = 0; byte_index < count; ++byte_index)
        {
            buffer[byte_index] = data[offset + byte_index];
        }

        buffers.push_back(std::move(buffer));
    }

    return buffers;
}
}
