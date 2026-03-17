#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"

#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_buffer_loader.hpp"
#include "legacy89diskkit/cpp/raw_disk_geometry.hpp"
#include "legacy89diskkit/cpp/disk_image_types.hpp"

namespace legacy89diskkit::cpp
{
Result<RawDiskContainer> RawDiskContainer::OpenFromBuffer(const std::span<const std::uint8_t> image_data, const bool read_only)
{
    const auto layout = RawBufferLoader::Load(image_data);
    if (!layout.ok())
    {
        return Result<RawDiskContainer>::Failure(layout.status().code, layout.status().message);
    }

    auto metadata = layout.value().metadata;
    metadata.is_write_protected = metadata.is_write_protected || read_only;
    return Result<RawDiskContainer>::Success(
        RawDiskContainer(std::vector<std::uint8_t>(image_data.begin(), image_data.end()), std::move(metadata), read_only, {}));
}

Result<RawDiskContainer> RawDiskContainer::CreateNew(DiskType type)
{
    std::size_t size = 0;
    int cylinders = 40;
    int heads = 2;
    int sectors_per_track = 16;
    int sector_size = 256;

    switch (type)
    {
    case DiskType::TwoD:
        size = 327680;
        cylinders = 40;
        heads = 2;
        sectors_per_track = 16;
        sector_size = 256;
        break;
    case DiskType::TwoDD:
        size = 720 * 1024;
        cylinders = 80;
        heads = 2;
        sectors_per_track = 9;
        sector_size = 512;
        break;
    case DiskType::TwoHD:
        size = 1440 * 1024;
        cylinders = 80;
        heads = 2;
        sectors_per_track = 18;
        sector_size = 512;
        break;
    default:
        return Result<RawDiskContainer>::Failure(StatusCode::InvalidArgument, "Unsupported disk type for raw creation.");
    }

    DiskContainerMetadata metadata{};
    metadata.disk_type = type;
    metadata.geometry = DiskGeometryInfo{cylinders, heads, sectors_per_track, sector_size};
    metadata.is_write_protected = false;
    metadata.image_format = "raw-sector-container";

    return Result<RawDiskContainer>::Success(
        RawDiskContainer(std::vector<std::uint8_t>(size, 0x00), std::move(metadata), false, {}));
}

RawDiskContainer::RawDiskContainer(
    std::vector<std::uint8_t> image_data,
    DiskContainerMetadata metadata,
    const bool read_only,
    std::string file_path)
    : image_data_(std::move(image_data)),
      metadata_(std::move(metadata)),
      read_only_(read_only),
      file_path_(std::move(file_path))
{
}

const std::string& RawDiskContainer::FilePath() const
{
    return file_path_;
}

bool RawDiskContainer::IsReadOnly() const
{
    return read_only_;
}

DiskType RawDiskContainer::DiskTypeValue() const
{
    return metadata_.disk_type;
}

DiskContainerMetadata RawDiskContainer::GetMetadata() const
{
    return metadata_;
}

Result<std::vector<std::uint8_t>> RawDiskContainer::ReadSector(const int cylinder, const int head, const int sector) const
{
    RawSectorAddressCalculator calculator(RawDiskGeometry{
        metadata_.geometry.cylinders,
        metadata_.geometry.heads,
        metadata_.geometry.sectors_per_track,
        metadata_.geometry.bytes_per_sector,
        metadata_.disk_type});

    const auto offset = calculator.CalculateOffset(cylinder, head, sector);
    if (!offset.ok())
    {
        return Result<std::vector<std::uint8_t>>::Failure(offset.status().code, offset.status().message);
    }

    const auto size = static_cast<std::size_t>(metadata_.geometry.bytes_per_sector);
    return Result<std::vector<std::uint8_t>>::Success(
        std::vector<std::uint8_t>(
            image_data_.begin() + offset.value(),
            image_data_.begin() + offset.value() + static_cast<std::ptrdiff_t>(size)));
}

Status RawDiskContainer::WriteSector(const int cylinder, const int head, const int sector, const std::span<const std::uint8_t> data)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "Raw disk container is read-only."};
    }

    if (data.size() > static_cast<std::size_t>(metadata_.geometry.bytes_per_sector))
    {
        return {StatusCode::InvalidArgument, "Data size exceeds sector size."};
    }

    RawSectorAddressCalculator calculator(RawDiskGeometry{
        metadata_.geometry.cylinders,
        metadata_.geometry.heads,
        metadata_.geometry.sectors_per_track,
        metadata_.geometry.bytes_per_sector,
        metadata_.disk_type});

    const auto offset = calculator.CalculateOffset(cylinder, head, sector);
    if (!offset.ok())
    {
        return offset.status();
    }

    std::copy(data.begin(), data.end(), image_data_.begin() + offset.value());
    dirty_ = true;
    return Status::OkStatus();
}

bool RawDiskContainer::SectorExists(const int cylinder, const int head, const int sector) const
{
    RawSectorAddressCalculator calculator(RawDiskGeometry{
        metadata_.geometry.cylinders,
        metadata_.geometry.heads,
        metadata_.geometry.sectors_per_track,
        metadata_.geometry.bytes_per_sector,
        metadata_.disk_type});
    return calculator.SectorExists(cylinder, head, sector);
}

std::vector<SectorInfo> RawDiskContainer::GetAllSectors() const
{
    std::vector<SectorInfo> sectors;
    sectors.reserve(static_cast<std::size_t>(metadata_.geometry.cylinders) *
        static_cast<std::size_t>(metadata_.geometry.heads) *
        static_cast<std::size_t>(metadata_.geometry.sectors_per_track));

    for (int cylinder = 0; cylinder < metadata_.geometry.cylinders; ++cylinder)
    {
        for (int head = 0; head < metadata_.geometry.heads; ++head)
        {
            for (int sector = 1; sector <= metadata_.geometry.sectors_per_track; ++sector)
            {
                sectors.push_back(SectorInfo{cylinder, head, sector, metadata_.geometry.bytes_per_sector, false, false});
            }
        }
    }

    return sectors;
}

bool RawDiskContainer::HasChanges() const
{
    return dirty_;
}

void RawDiskContainer::ResetChanges()
{
    dirty_ = false;
}

std::vector<std::uint8_t> RawDiskContainer::ToImageData() const
{
    return image_data_;
}
}
