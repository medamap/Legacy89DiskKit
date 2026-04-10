#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicReadRules
{
public:
    static std::vector<std::uint8_t> TrimToRecordedLength(const std::vector<std::uint8_t>& data, const HuBasicFileEntry& file_entry);
    static std::vector<std::uint8_t> TrimToTerminalLength(
        const std::vector<std::uint8_t>& data,
        DiskType disk_type,
        const HuBasicConfiguration& config,
        int cluster_count,
        int terminal_flag,
        std::uint32_t recorded_size);
    static std::vector<std::uint8_t> ExtractAsciiPayload(const std::vector<std::uint8_t>& data);
    static std::vector<std::uint8_t> ResolveReadPayload(
        const std::vector<std::uint8_t>& data,
        const HuBasicFileEntry& file_entry,
        DiskType disk_type,
        const HuBasicConfiguration& config,
        int cluster_count,
        int terminal_flag);
};
}
