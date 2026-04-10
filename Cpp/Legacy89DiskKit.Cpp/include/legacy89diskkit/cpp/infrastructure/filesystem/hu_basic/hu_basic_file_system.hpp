#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_layout_types.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string>
#include <string_view>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicFileSystem
{
public:
    static HuBasicFileSystem Open(RawDiskContainer& container);
    static HuBasicFileSystem Open(D88DiskContainer& container);

    const HuBasicConfiguration& GetConfiguration() const;
    DiskType DiskTypeValue() const;
    bool IsReadOnly() const;

    HuBasicFileSystemInfo GetFileSystemInfo() const;
    std::vector<HuBasicFileEntry> GetFiles() const;
    bool FileExists(std::string_view file_name) const;
    Result<std::vector<std::uint8_t>> ReadFile(std::string_view file_name) const;

    Status WriteFile(
        std::string_view file_name,
        const std::vector<std::uint8_t>& data,
        const HuBasicFileAttributes& attributes,
        std::uint16_t load_address = 0,
        std::uint16_t execution_address = 0);

    Status DeleteFile(std::string_view file_name);
    Status RenameFile(std::string_view old_name, std::string_view new_name);
    Status UpdateAttributes(std::string_view file_name, const HuBasicFileAttributes& attributes);
    Status Format();

    HuBasicDirectoryLayout ReadDirectoryLayout() const;
    Status ApplyDirectoryLayout(const HuBasicDirectoryLayout& layout);

    Result<std::vector<std::uint8_t>> ReadBootArea() const;
    Status WriteBootArea(const std::vector<std::uint8_t>& data);

private:
    HuBasicFileSystem(
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

    RawDiskContainer* raw_container_;
    D88DiskContainer* d88_container_;
    HuBasicConfiguration config_;
    DiskType disk_type_;
    bool read_only_;
};
}
