#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"

#include <cstdint>

namespace legacy89diskkit::cpp
{
struct HuBasicConfiguration
{
    int reserved_clusters;
    int total_clusters;
    int cluster_size;
    int sector_size;
};

struct HuBasicFileAttributes
{
    bool is_ascii;
};

struct HuBasicFileEntry
{
    std::uint32_t size;
    HuBasicFileAttributes attributes;
};
}
