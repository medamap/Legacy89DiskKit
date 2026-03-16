#pragma once

#include "legacy89diskkit/cpp/msx_dos_types.hpp"

#include <optional>
#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
struct MsxDosWriteTransactionPlan
{
    std::vector<std::uint8_t> payload;
    std::vector<int> allocated_clusters;
    MsxDosFileEntry file_entry;
};

class MsxDosWriteTransaction
{
public:
    static std::optional<MsxDosWriteTransactionPlan> CreatePlan(
        const std::string& file_name,
        const std::vector<std::uint8_t>& data,
        const MsxDosFileAttributes& attributes,
        const MsxDosConfiguration& config,
        const std::vector<std::uint8_t>& fat_data);
};
}
