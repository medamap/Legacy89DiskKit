#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry.hpp"
#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
struct HuBasicWriteTransactionPlan
{
    std::vector<std::uint8_t> payload;
    std::vector<int> allocated_clusters;
    int terminal_flag;
    HuBasicFileEntry file_entry;
    HuBasicDirectoryEntry directory_entry;
};

class HuBasicWriteTransaction
{
public:
    static std::optional<HuBasicWriteTransactionPlan> CreatePlan(
        const std::string& file_name,
        const std::vector<std::uint8_t>& data,
        const HuBasicFileAttributes& attributes,
        DiskType disk_type,
        const HuBasicConfiguration& config,
        const std::vector<std::uint8_t>& fat_data,
        std::uint16_t load_address,
        std::uint16_t execution_address);
};
}
