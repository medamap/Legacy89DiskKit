#include "legacy89diskkit/cpp/raw_disk_geometry.hpp"

namespace legacy89diskkit::cpp
{
RawDiskGeometry RawDiskGeometryDetector::Detect(std::uint64_t size)
{
    switch (size)
    {
    case 327680:
        return {40, 2, 16, 256, DiskType::TwoD};
    case 655360:
        return {80, 2, 16, 256, DiskType::TwoDD};
    case 737280:
        return {80, 2, 9, 512, DiskType::TwoDD};
    case 1261568:
        return {77, 2, 8, 1024, DiskType::TwoHD};
    case 1474560:
        return {80, 2, 18, 512, DiskType::TwoHD};
    default:
        return {40, 2, 16, 256, DiskType::TwoD};
    }
}

RawSectorAddressCalculator::RawSectorAddressCalculator(RawDiskGeometry geometry)
    : geometry_(geometry)
{
}

bool RawSectorAddressCalculator::SectorExists(int cylinder, int head, int sector) const
{
    return cylinder >= 0 && cylinder < geometry_.cylinders &&
           head >= 0 && head < geometry_.sides &&
           sector >= 1 && sector <= geometry_.sectors_per_track;
}

Result<int> RawSectorAddressCalculator::CalculateOffset(int cylinder, int head, int sector) const
{
    if (!SectorExists(cylinder, head, sector))
    {
        return Result<int>::Failure(StatusCode::OutOfRange, "Invalid sector address.");
    }

    const auto offset =
        ((cylinder * geometry_.sides + head) * geometry_.sectors_per_track + (sector - 1)) *
        geometry_.bytes_per_sector;
    return Result<int>::Success(offset);
}
}
