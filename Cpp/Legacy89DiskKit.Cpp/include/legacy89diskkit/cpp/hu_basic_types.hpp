#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"

#include <cstdint>

namespace legacy89diskkit::cpp
{
struct HuBasicConfiguration
{
    int total_tracks;
    int sectors_per_track;
    int fat_track;
    int fat_sector;
    int fat_sectors;
    int directory_track;
    int directory_sector;
    int directory_sectors;
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
    bool has_password;
    bool is_hidden;
    bool is_verify;
    bool is_write_protected;
    bool is_directory;
    std::uint16_t recorded_size;
    std::uint16_t load_address;
    std::uint16_t execution_address;
    int start_cluster;
    std::uint8_t raw_mode_byte;
    std::uint8_t password_byte;
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
    HuBasicFileMetadata metadata;
};
}
