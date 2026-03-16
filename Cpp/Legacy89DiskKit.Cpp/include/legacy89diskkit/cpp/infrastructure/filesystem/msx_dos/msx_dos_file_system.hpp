#pragma once

#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/msx_dos_types.hpp"
#include "legacy89diskkit/cpp/msx_dos_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string_view>
#include <vector>

namespace legacy89diskkit::cpp
{
class MsxDosFileSystem
{
public:
    MsxDosFileSystem() = default;

    static Result<MsxDosFileSystem> Open(RawDiskContainer& container);
    static Result<MsxDosFileSystem> Open(D88DiskContainer& container);
    static MsxDosFileSystem OpenExplicit(RawDiskContainer& container);
    static MsxDosFileSystem OpenExplicit(D88DiskContainer& container);

    const MsxDosConfiguration& GetConfiguration() const;
    DiskType DiskTypeValue() const;
    bool IsReadOnly() const;

    MsxDosFileSystemInfo GetFileSystemInfo() const;
    std::vector<MsxDosFileEntry> GetFiles() const;
    bool FileExists(std::string_view file_name) const;
    Result<std::vector<std::uint8_t>> ReadFile(std::string_view file_name) const;

    Status WriteFile(
        std::string_view file_name,
        const std::vector<std::uint8_t>& data,
        const MsxDosFileAttributes& attributes);

    Status DeleteFile(std::string_view file_name);
    Status RenameFile(std::string_view old_name, std::string_view new_name);
    Status UpdateAttributes(std::string_view file_name, const MsxDosFileAttributes& attributes);
    Status Format();

    Result<std::vector<std::uint8_t>> ReadBootSector() const;
    Status WriteBootSector(const std::vector<std::uint8_t>& sector_data);

private:
    MsxDosFileSystem(
        RawDiskContainer* raw_container,
        D88DiskContainer* d88_container,
        MsxDosConfiguration config,
        DiskType disk_type,
        bool read_only);

    Result<std::vector<std::uint8_t>> ReadSector(int cylinder, int head, int sector) const;
    Status WriteSector(int cylinder, int head, int sector, const std::vector<std::uint8_t>& data);
    std::vector<std::uint8_t> ReadFat() const;
    Status WriteFat(const std::vector<std::uint8_t>& fat_data);
    std::vector<std::vector<std::uint8_t>> ReadRootDirectorySectors() const;
    Status WriteRootDirectorySector(int sector_index, const std::vector<std::uint8_t>& sector_data);
    Result<std::vector<std::uint8_t>> ReadCluster(int cluster) const;
    Status WriteCluster(int cluster, const std::vector<std::uint8_t>& cluster_data);
    static void LbaToPhysical(const MsxDosConfiguration& config, int lba, int& cylinder, int& head, int& sector);
    static std::array<std::uint8_t, 32> EncodeDirectoryEntry(const MsxDosFileEntry& entry);

    RawDiskContainer* raw_container_;
    D88DiskContainer* d88_container_;
    MsxDosConfiguration config_;
    DiskType disk_type_;
    bool read_only_;
};
}
