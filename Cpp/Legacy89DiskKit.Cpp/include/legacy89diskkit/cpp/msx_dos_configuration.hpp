#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"

#include <cstdint>

namespace legacy89diskkit::cpp
{
struct MsxDosConfiguration
{
    int sector_size;
    int sectors_per_cluster;
    int reserved_sectors;
    int number_of_fats;
    int root_directory_entries;
    int sectors_per_fat;
    int sectors_per_track;
    int number_of_heads;
    int total_sectors;
    std::uint8_t media_descriptor;

    [[nodiscard]] int ClusterSize() const noexcept;
    [[nodiscard]] int RootDirectorySectors() const noexcept;
    [[nodiscard]] int FirstDataSector() const noexcept;
    [[nodiscard]] int TotalClusters() const noexcept;
    [[nodiscard]] int ClusterToLba(int cluster_number) const noexcept;
    [[nodiscard]] int GetFatStartSector(int fat_number) const noexcept;
    [[nodiscard]] int GetRootDirectoryStartSector() const noexcept;
};

class MsxDosConfigurationProvider
{
public:
    static MsxDosConfiguration GetDefault(DiskType disk_type);
};
}
