#include "legacy89diskkit/cpp/msx_dos_write_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<std::uint8_t> MsxDosWriteRules::PrepareWritePayload(
    const std::vector<std::uint8_t>& data,
    const MsxDosFileAttributes& attributes)
{
    auto payload = data;
    if (attributes.is_ascii)
    {
        payload.push_back(0x1a);
    }

    return payload;
}

int MsxDosWriteRules::GetClustersNeeded(const int payload_size, const MsxDosConfiguration& config)
{
    if (payload_size <= 0)
    {
        return 1;
    }

    return (payload_size + config.ClusterSize() - 1) / config.ClusterSize();
}
}
