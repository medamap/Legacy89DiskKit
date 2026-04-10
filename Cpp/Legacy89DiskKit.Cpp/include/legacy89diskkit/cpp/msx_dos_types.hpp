#pragma once

#include <array>
#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
{
struct MsxDosFileAttributes
{
    bool is_ascii;
    std::uint8_t raw_attributes;
    bool is_read_only;
    bool is_hidden;
    bool is_system;
    bool is_directory;
    bool is_archive;
};

struct MsxDosFileEntry
{
    std::string file_name;
    std::string extension;
    std::uint32_t size;
    MsxDosFileAttributes attributes;
    int start_cluster;
    std::uint16_t write_time;
    std::uint16_t write_date;
    std::array<std::uint8_t, 8> raw_file_name;
    std::array<std::uint8_t, 3> raw_extension;
};

struct MsxDosFileSystemInfo
{
    std::int64_t total_size;
    std::int64_t free_space;
    int cluster_size;
    int first_data_sector;
};
}
