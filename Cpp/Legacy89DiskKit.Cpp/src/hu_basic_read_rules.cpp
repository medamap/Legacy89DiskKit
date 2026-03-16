#include "legacy89diskkit/cpp/hu_basic_read_rules.hpp"

#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<std::uint8_t> HuBasicReadRules::TrimToRecordedLength(const std::vector<std::uint8_t>& data, const HuBasicFileEntry& file_entry)
{
    if (file_entry.size > 0 && data.size() > file_entry.size)
    {
        return std::vector<std::uint8_t>(data.begin(), data.begin() + static_cast<std::ptrdiff_t>(file_entry.size));
    }

    return data;
}

std::vector<std::uint8_t> HuBasicReadRules::TrimToTerminalLength(
    const std::vector<std::uint8_t>& data,
    DiskType disk_type,
    const HuBasicConfiguration& config,
    int cluster_count,
    int terminal_flag,
    std::uint32_t recorded_size)
{
    if (disk_type != DiskType::TwoHD)
    {
        return data;
    }

    const auto used_in_last = HuBasicFatRules::GetLastClusterUsedSectors(terminal_flag);
    if (used_in_last == 0)
    {
        return data;
    }

    const auto sectors_per_cluster = config.cluster_size / config.sector_size;
    const auto total_records = (cluster_count - 1) * sectors_per_cluster + used_in_last;
    const auto total_bytes = total_records * config.sector_size;

    if (recorded_size == 0 || total_bytes < static_cast<int>(data.size()))
    {
        return std::vector<std::uint8_t>(data.begin(), data.begin() + total_bytes);
    }

    return data;
}

std::vector<std::uint8_t> HuBasicReadRules::ExtractAsciiPayload(const std::vector<std::uint8_t>& data)
{
    std::vector<std::uint8_t> result;
    result.reserve(data.size());

    for (const auto value : data)
    {
        if (value == 0x1a)
        {
            break;
        }

        result.push_back(value);
    }

    return result;
}

std::vector<std::uint8_t> HuBasicReadRules::ResolveReadPayload(
    const std::vector<std::uint8_t>& data,
    const HuBasicFileEntry& file_entry,
    DiskType disk_type,
    const HuBasicConfiguration& config,
    int cluster_count,
    int terminal_flag)
{
    auto adjusted = TrimToTerminalLength(data, disk_type, config, cluster_count, terminal_flag, file_entry.size);
    if (file_entry.attributes.is_ascii)
    {
        return ExtractAsciiPayload(adjusted);
    }

    return TrimToRecordedLength(adjusted, file_entry);
}
}
