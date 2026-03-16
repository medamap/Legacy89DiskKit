#include "legacy89diskkit/cpp/hu_basic_write_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_mode_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_write_rules.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
namespace
{
template <std::size_t Size>
std::array<std::uint8_t, Size> EncodePaddedText(const std::string& text)
{
    std::array<std::uint8_t, Size> bytes{};
    bytes.fill(static_cast<std::uint8_t>(' '));

    const auto count = std::min(Size, text.size());
    for (std::size_t index = 0; index < count; ++index)
    {
        bytes[index] = static_cast<std::uint8_t>(text[index]);
    }

    return bytes;
}
}

std::optional<HuBasicWriteTransactionPlan> HuBasicWriteTransaction::CreatePlan(
    const std::string& file_name,
    const std::vector<std::uint8_t>& data,
    const HuBasicFileAttributes& attributes,
    const DiskType disk_type,
    const HuBasicConfiguration& config,
    const std::vector<std::uint8_t>& fat_data,
    const std::uint16_t load_address,
    const std::uint16_t execution_address)
{
    const auto payload = HuBasicWriteRules::PrepareWritePayload(data, attributes);
    const auto clusters_needed = HuBasicWriteRules::GetClustersNeeded(static_cast<int>(payload.size()), config);
    const auto allocated_clusters = HuBasicAllocationRules::CollectFreeClusters(fat_data, disk_type, config, clusters_needed);
    if (static_cast<int>(allocated_clusters.size()) < clusters_needed)
    {
        return std::nullopt;
    }

    const auto terminal_flag = HuBasicWriteRules::GetTerminalFlagForLength(static_cast<int>(payload.size()), config);
    const auto file_entry = HuBasicDirectoryRules::CreateFileEntryForWrite(
        file_name,
        payload,
        attributes,
        allocated_clusters.front(),
        load_address,
        execution_address);
    const auto mode_byte = HuBasicModeRules::BuildModeByte(file_entry.metadata);

    return HuBasicWriteTransactionPlan
    {
        payload,
        allocated_clusters,
        terminal_flag,
        file_entry,
        HuBasicDirectoryEntry
        {
            mode_byte,
            file_entry.metadata.password_byte,
            EncodePaddedText<13>(file_entry.file_name),
            EncodePaddedText<3>(file_entry.extension),
            file_entry.file_name,
            file_entry.extension,
            file_entry.metadata.recorded_size,
            file_entry.load_address,
            file_entry.execution_address,
            file_entry.start_cluster
        }
    };
}
}
