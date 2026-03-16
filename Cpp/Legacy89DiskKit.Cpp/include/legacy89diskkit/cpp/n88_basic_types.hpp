#pragma once

#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
{
struct N88BasicConfiguration
{
    int system_track;
    int system_head;
    int directory_sector;
    int directory_sectors;
    int fat_sector;
    int fat_sectors;
    int id_sector;
    int sector_size;
    int cluster_size;
    int total_clusters;
    int reserved_clusters;
    int sectors_per_track;
};

struct N88BasicFileAttributes
{
    bool is_ascii;
    std::uint8_t raw_attributes;
    bool is_read_only;
};

struct N88BasicFileEntry
{
    std::string file_name;
    std::string extension;
    std::uint32_t size;
    N88BasicFileAttributes attributes;
    int start_cluster;
};
}
