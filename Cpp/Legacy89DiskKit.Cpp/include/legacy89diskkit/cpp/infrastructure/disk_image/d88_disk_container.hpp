#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <span>
#include <string>
#include <vector>

namespace legacy89diskkit::cpp
{
class D88DiskContainer
{
public:
    D88DiskContainer() = default;

    static Result<D88DiskContainer> OpenFromBuffer(std::span<const std::uint8_t> image_data, bool read_only = true);
    static Result<D88DiskContainer> CreateNew(DiskType type, const std::string& name);

    const std::string& FilePath() const;
    bool IsReadOnly() const;
    DiskType DiskTypeValue() const;
    DiskContainerMetadata GetMetadata() const;

    Result<std::vector<std::uint8_t>> ReadSector(int cylinder, int head, int sector) const;
    Status WriteSector(int cylinder, int head, int sector, std::span<const std::uint8_t> data);

    bool SectorExists(int cylinder, int head, int sector) const;
    std::vector<SectorInfo> GetAllSectors() const;
    std::vector<std::uint8_t> ToImageData() const;
    bool HasChanges() const;
    void ResetChanges();

private:
    D88DiskContainer(
        std::string image_name,
        std::vector<SectorDataBlock> sectors,
        DiskContainerMetadata metadata,
        bool read_only,
        std::string file_path);

    std::string image_name_;
    std::vector<SectorDataBlock> sectors_;
    DiskContainerMetadata metadata_;
    bool read_only_{true};
    std::string file_path_;
    bool dirty_{false};
};
}
