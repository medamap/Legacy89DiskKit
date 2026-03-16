#include "legacy89diskkit/cpp/msx_dos_write_transaction.hpp"

#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_allocation_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_write_rules.hpp"

namespace legacy89diskkit::cpp
{
MsxDosWriteTransaction::CreatePlan(
    const std::string& file_name,
    const std::vector<std::uint8_t>& data,
    const MsxDosFileAttributes& attributes,
    const MsxDosConfiguration& config,
    const std::vector<std::uint8_t>& fat_data) -> std::optional<MsxDosWriteTransactionPlan>
{
    const auto parsed = HuBasicNameRules::ParseFileName(file_name);
    const auto payload = MsxDosWriteRules::PrepareWritePayload(data, attributes);
    const auto clusters_needed = MsxDosWriteRules::GetClustersNeeded(static_cast<int>(payload.size()), config);
    const auto allocated_clusters = MsxDosAllocationRules::CollectFreeClusters(fat_data, config, clusters_needed);
    if (static_cast<int>(allocated_clusters.size()) < clusters_needed)
    {
        return std::nullopt;
    }

    return MsxDosWriteTransactionPlan{
        payload,
        allocated_clusters,
        MsxDosFileEntry{
            parsed.file_name,
            parsed.extension,
            static_cast<std::uint32_t>(payload.size()),
            attributes,
            allocated_clusters.front(),
            0,
            0,
            {},
            {} } };
}
}
