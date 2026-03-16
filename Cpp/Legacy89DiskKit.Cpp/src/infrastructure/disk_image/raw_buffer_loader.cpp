#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_buffer_loader.hpp"

#include "legacy89diskkit/cpp/raw_disk_geometry.hpp"

namespace legacy89diskkit::cpp
{
Result<ReadOnlyDiskImageLayout> RawBufferLoader::Load(const std::span<const std::uint8_t> image_data)
{
    if (image_data.empty())
    {
        return Result<ReadOnlyDiskImageLayout>::Failure(StatusCode::InvalidArgument, "Image buffer must not be empty.");
    }

    const auto geometry = RawDiskGeometryDetector::Detect(image_data.size());
    const auto total_sectors = static_cast<std::size_t>(geometry.cylinders) *
        static_cast<std::size_t>(geometry.sides) *
        static_cast<std::size_t>(geometry.sectors_per_track);
    const auto expected_size = total_sectors * static_cast<std::size_t>(geometry.bytes_per_sector);
    if (expected_size != image_data.size())
    {
        return Result<ReadOnlyDiskImageLayout>::Failure(StatusCode::UnsupportedFormat, "Raw image size does not match a supported geometry.");
    }

    std::vector<SectorDataBlock> sectors;
    sectors.reserve(total_sectors);

    std::size_t offset = 0;
    for (int cylinder = 0; cylinder < geometry.cylinders; ++cylinder)
    {
        for (int head = 0; head < geometry.sides; ++head)
        {
            for (int sector = 1; sector <= geometry.sectors_per_track; ++sector)
            {
                sectors.push_back(SectorDataBlock{
                    SectorInfo{cylinder, head, sector, geometry.bytes_per_sector, false, false},
                    std::vector<std::uint8_t>(
                        image_data.begin() + static_cast<std::ptrdiff_t>(offset),
                        image_data.begin() + static_cast<std::ptrdiff_t>(offset + geometry.bytes_per_sector))});
                offset += static_cast<std::size_t>(geometry.bytes_per_sector);
            }
        }
    }

    return Result<ReadOnlyDiskImageLayout>::Success(ReadOnlyDiskImageLayout{
        DiskContainerMetadata{
            "raw-sector-container",
            geometry.disk_type,
            DiskGeometryInfo{geometry.cylinders, geometry.sides, geometry.sectors_per_track, geometry.bytes_per_sector},
            false,
            static_cast<std::uint32_t>(image_data.size())},
        std::move(sectors)});
}
}
