#pragma once

#include <cstdint>
#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
enum class DiskType : std::uint8_t
{
    TwoD = 0x00,
    TwoDD = 0x10,
    TwoHD = 0x20,
    HardDisk = 0x80
};

struct SectorInfo
{
    int cylinder;
    int head;
    int sector;
    int size;
    bool is_deleted;
    bool has_error;
};

struct DiskGeometryInfo
{
    int cylinders;
    int heads;
    int sectors_per_track;
    int bytes_per_sector;
};

struct DiskContainerMetadata
{
    std::string image_format;
    DiskType disk_type;
    DiskGeometryInfo geometry;
    bool is_write_protected;
    std::uint32_t declared_image_size;
};

struct SectorDataBlock
{
    SectorInfo sector;
    std::vector<std::uint8_t> data;
};

struct ReadOnlyDiskImageLayout
{
    DiskContainerMetadata metadata;
    std::vector<SectorDataBlock> sectors;
};
}
