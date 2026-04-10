#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <stdexcept>

namespace legacy89diskkit::cpp
{
int MsxDosConfiguration::ClusterSize() const noexcept
{
    return sector_size * sectors_per_cluster;
}

int MsxDosConfiguration::RootDirectorySectors() const noexcept
{
    return (root_directory_entries * 32 + sector_size - 1) / sector_size;
}

int MsxDosConfiguration::FirstDataSector() const noexcept
{
    return reserved_sectors + (number_of_fats * sectors_per_fat) + RootDirectorySectors();
}

int MsxDosConfiguration::TotalClusters() const noexcept
{
    return (total_sectors - FirstDataSector()) / sectors_per_cluster;
}

int MsxDosConfiguration::ClusterToLba(const int cluster_number) const noexcept
{
    return FirstDataSector() + (cluster_number - 2) * sectors_per_cluster;
}

int MsxDosConfiguration::GetFatStartSector(const int fat_number) const noexcept
{
    return reserved_sectors + (fat_number * sectors_per_fat);
}

int MsxDosConfiguration::GetRootDirectoryStartSector() const noexcept
{
    return reserved_sectors + (number_of_fats * sectors_per_fat);
}

MsxDosConfiguration MsxDosConfigurationProvider::GetDefault(const DiskType disk_type)
{
    switch (disk_type)
    {
    case DiskType::TwoDD:
        return MsxDosConfiguration{ 512, 2, 1, 2, 112, 9, 9, 2, 1440, 0xf9 };
    case DiskType::TwoD:
        return MsxDosConfiguration{ 512, 2, 1, 2, 112, 5, 9, 1, 720, 0xf9 };
    case DiskType::TwoHD:
        return MsxDosConfiguration{ 512, 1, 1, 2, 224, 9, 18, 2, 2880, 0xf0 };
    default:
        throw std::invalid_argument("Unsupported disk type for MSX-DOS");
    }
}
}
