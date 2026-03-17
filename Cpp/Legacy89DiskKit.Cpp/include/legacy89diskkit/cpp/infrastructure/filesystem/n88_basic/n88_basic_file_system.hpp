#pragma once

#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/n88_basic_types.hpp"
#include "legacy89diskkit/cpp/n88_basic_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string_view>
#include <vector>

namespace legacy89diskkit::cpp
{
class N88BasicFileSystem
{
public:
    static N88BasicFileSystem Open(RawDiskContainer& container);
    static N88BasicFileSystem Open(D88DiskContainer& container);

    const N88BasicConfiguration& GetConfiguration() const;
    DiskType DiskTypeValue() const;
    bool IsReadOnly() const;

    N88BasicFileSystemInfo GetFileSystemInfo() const;
    std::vector<N88BasicFileEntry> GetFiles() const;
    bool FileExists(std::string_view file_name) const;
    Result<std::vector<std::uint8_t>> ReadFile(std::string_view file_name) const;

    Status WriteFile(
        std::string_view file_name,
        const std::vector<std::uint8_t>& data,
        const N88BasicFileAttributes& attributes);

    Status DeleteFile(std::string_view file_name);
    Status RenameFile(std::string_view old_name, std::string_view new_name);
    Status UpdateAttributes(std::string_view file_name, const N88BasicFileAttributes& attributes);
    Status Format();

    Result<std::vector<std::uint8_t>> ReadBootArea() const;
    Status WriteBootArea(const std::vector<std::uint8_t>& data);

private:
    N88BasicFileSystem(
        RawDiskContainer* raw_container,
        D88DiskContainer* d88_container,
        DiskType disk_type,
        bool read_only);

    Result<std::vector<std::uint8_t>> ReadSector(int cylinder, int head, int sector) const;
    Status WriteSector(int cylinder, int head, int sector, const std::vector<std::uint8_t>& data);
    std::vector<std::uint8_t> ReadFat() const;
    Status WriteFat(const std::vector<std::uint8_t>& fat_data);
    std::vector<std::vector<std::uint8_t>> ReadDirectorySectors() const;
    Status WriteDirectorySector(int sector_index, const std::vector<std::uint8_t>& sector_data);
    Result<std::vector<std::uint8_t>> ReadCluster(int cluster) const;
    Status WriteCluster(int cluster, const std::vector<std::uint8_t>& cluster_data);
    static void GetPhysicalAddress(int track, int sector, int& cylinder, int& head, int& physical_sector);

    RawDiskContainer* raw_container_;
    D88DiskContainer* d88_container_;
    N88BasicConfiguration config_;
    DiskType disk_type_;
    bool read_only_;
};
}
