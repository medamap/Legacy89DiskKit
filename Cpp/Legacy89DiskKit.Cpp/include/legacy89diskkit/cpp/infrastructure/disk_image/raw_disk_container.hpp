#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <span>
#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
class RawDiskContainer
{
public:
    RawDiskContainer() = default;

    static Result<RawDiskContainer> OpenFromBuffer(std::span<const std::uint8_t> image_data, bool read_only = true);

    const std::string& FilePath() const;
    bool IsReadOnly() const;
    DiskType DiskTypeValue() const;
    DiskContainerMetadata GetMetadata() const;

    Result<std::vector<std::uint8_t>> ReadSector(int cylinder, int head, int sector) const;
    Status WriteSector(int cylinder, int head, int sector, std::span<const std::uint8_t> data);

    bool SectorExists(int cylinder, int head, int sector) const;
    std::vector<SectorInfo> GetAllSectors() const;
    std::vector<std::uint8_t> ToImageData() const;

private:
    RawDiskContainer(
        std::vector<std::uint8_t> image_data,
        DiskContainerMetadata metadata,
        bool read_only,
        std::string file_path);

    std::vector<std::uint8_t> image_data_;
    DiskContainerMetadata metadata_;
    bool read_only_;
    std::string file_path_;
};
}
