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
    std::uint8_t raw_attributes;
    bool is_directory;
    bool is_read_only;
    bool is_hidden;
};

struct HuBasicFileEntry
{
    std::string file_name;
    std::string extension;
    std::uint32_t size;
    HuBasicFileAttributes attributes;
    int start_cluster;
    std::uint16_t load_address;
    std::uint16_t end_address;
    std::uint16_t execution_address;
};

enum class HuBasicFileType : std::uint8_t
{
    Unknown = 0,
    Binary = 1,
    Basic = 2,
    Ascii = 3,
};

struct HuBasicFileMetadata
{
    HuBasicFileType file_type;
    std::uint16_t recorded_size;
    std::uint16_t load_address;
    std::uint16_t execution_address;
    int start_cluster;
    std::uint8_t raw_mode_byte;
};
}
