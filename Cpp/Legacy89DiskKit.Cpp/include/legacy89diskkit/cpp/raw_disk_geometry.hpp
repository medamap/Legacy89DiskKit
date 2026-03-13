#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/status.hpp"

namespace legacy89diskkit::cpp
{
struct RawDiskGeometry
{
    int cylinders;
    int sides;
    int sectors_per_track;
    int bytes_per_sector;
    DiskType disk_type;
};

class RawDiskGeometryDetector
{
public:
    static RawDiskGeometry Detect(std::uint64_t size);
};

class RawSectorAddressCalculator
{
public:
    explicit RawSectorAddressCalculator(RawDiskGeometry geometry);

    [[nodiscard]] bool SectorExists(int cylinder, int head, int sector) const;
    [[nodiscard]] Result<int> CalculateOffset(int cylinder, int head, int sector) const;

private:
    RawDiskGeometry geometry_;
};
}
