#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
struct HuBasicFileSystemInfo
{
    std::int64_t total_size;
    std::int64_t free_space;
    int cluster_size;
    int reserved_sectors;
};

class HuBasicFileSystemInfoRules
{
public:
    static int CountFreeClusters(
        const std::vector<std::uint8_t>& fat_data,
        DiskType disk_type,
        const HuBasicConfiguration& config);

    static HuBasicFileSystemInfo BuildInfo(
        const std::vector<std::uint8_t>& fat_data,
        DiskType disk_type,
        const HuBasicConfiguration& config);
};
}
