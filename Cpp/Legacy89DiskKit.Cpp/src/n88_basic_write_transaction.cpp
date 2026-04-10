#include "legacy89diskkit/cpp/n88_basic_write_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_write_rules.hpp"

namespace legacy89diskkit::cpp
{
std::optional<N88BasicWriteTransactionPlan> N88BasicWriteTransaction::CreatePlan(
    const std::string& file_name,
    const std::vector<std::uint8_t>& data,
    const N88BasicFileAttributes& attributes,
    const N88BasicConfiguration& config,
    const std::vector<std::uint8_t>& fat_data)
{
    const auto parsed = HuBasicNameRules::ParseFileName(file_name);
    const auto payload = N88BasicWriteRules::PrepareWritePayload(data, attributes);
    const auto clusters_needed = N88BasicWriteRules::GetClustersNeeded(static_cast<int>(payload.size()), config);
    const auto allocated_clusters = N88BasicAllocationRules::CollectFreeClusters(fat_data, config, clusters_needed);
    if (static_cast<int>(allocated_clusters.size()) < clusters_needed)
    {
        return std::nullopt;
    }

    return N88BasicWriteTransactionPlan{
        payload,
        allocated_clusters,
        N88BasicWriteRules::GetTerminalFlagForLength(static_cast<int>(payload.size()), config),
        N88BasicFileEntry{
            parsed.file_name,
            parsed.extension,
            static_cast<std::uint32_t>(payload.size()),
            attributes,
            allocated_clusters.front() } };
}
}
