#include "legacy89diskkit/cpp/msx_dos_read_rules.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
std::uint32_t MsxDosReadRules::ResolveSizeFromFat(
    const std::vector<int>& clusters,
    const MsxDosConfiguration& config,
    const std::uint32_t declared_size)
{
    const auto maximum_size = static_cast<std::uint32_t>(clusters.size() * config.ClusterSize());
    return std::min(maximum_size, declared_size);
}

std::vector<std::uint8_t> MsxDosReadRules::ResolveReadPayload(
    const std::vector<std::uint8_t>& data,
    const MsxDosFileEntry& file)
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
