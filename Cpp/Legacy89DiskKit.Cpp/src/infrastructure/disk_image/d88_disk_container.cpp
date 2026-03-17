#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"

#include "legacy89diskkit/cpp/d88_parser.hpp"

#include <algorithm>
#include <array>

namespace legacy89diskkit::cpp
{
namespace
{
std::uint8_t DetermineSectorSizeCode(const int size)
{
    switch (size)
    {
    case 128:
        return 0;
    case 256:
        return 1;
    case 512:
        return 2;
    case 1024:
        return 3;
    default:
        return 0;
    }
}
}

Result<D88DiskContainer> D88DiskContainer::OpenFromBuffer(const std::span<const std::uint8_t> image_data, const bool read_only)
{
    const auto header_result = D88Parser::ParseHeader(std::vector<std::uint8_t>(image_data.begin(), image_data.end()));
    if (!header_result.ok())
    {
        return Result<D88DiskContainer>::Failure(header_result.status().code, header_result.status().message);
    }

    const auto layout_result = D88Parser::ParseImage(std::vector<std::uint8_t>(image_data.begin(), image_data.end()));
    if (!layout_result.ok())
    {
        return Result<D88DiskContainer>::Failure(layout_result.status().code, layout_result.status().message);
    }

    auto metadata = layout_result.value().metadata;
    metadata.is_write_protected = metadata.is_write_protected || read_only;
    return Result<D88DiskContainer>::Success(D88DiskContainer(
        header_result.value().image_name,
        layout_result.value().sectors,
        std::move(metadata),
        read_only,
        {}));
}

Result<D88DiskContainer> D88DiskContainer::CreateNew(DiskType type, const std::string& name)
{
    const int cylinders = (type == DiskType::TwoHD) ? 77 : 40;
    const int heads = 2;
    const int sectors_per_track = (type == DiskType::TwoHD) ? 26 : 16;
    const int sector_size = (type == DiskType::TwoHD) ? 1024 : 256;

    std::vector<SectorDataBlock> sectors;
    sectors.reserve(static_cast<std::size_t>(cylinders * heads * sectors_per_track));

    for (int c = 0; c < cylinders; ++c)
    {
        for (int h = 0; h < heads; ++h)
        {
            for (int s = 1; s <= sectors_per_track; ++s)
            {
                sectors.push_back(SectorDataBlock{
                    SectorInfo{c, h, s, sector_size, false, false},
                    std::vector<std::uint8_t>(static_cast<std::size_t>(sector_size), 0x00)});
            }
        }
    }

    DiskContainerMetadata metadata{};
    metadata.image_format = "d88-sector-container";
    metadata.disk_type = type;
    metadata.geometry = DiskGeometryInfo{cylinders, heads, sectors_per_track, sector_size};
    metadata.is_write_protected = false;

    return Result<D88DiskContainer>::Success(D88DiskContainer(
        name,
        std::move(sectors),
        std::move(metadata),
        false,
        {}));
}

D88DiskContainer::D88DiskContainer(
    std::string image_name,
    std::vector<SectorDataBlock> sectors,
    DiskContainerMetadata metadata,
    const bool read_only,
    std::string file_path)
    : image_name_(std::move(image_name)),
      sectors_(std::move(sectors)),
      metadata_(std::move(metadata)),
      read_only_(read_only),
      file_path_(std::move(file_path))
{
}

const std::string& D88DiskContainer::FilePath() const
{
    return file_path_;
}

bool D88DiskContainer::IsReadOnly() const
{
    return read_only_;
}

DiskType D88DiskContainer::DiskTypeValue() const
{
    return metadata_.disk_type;
}

DiskContainerMetadata D88DiskContainer::GetMetadata() const
{
    return metadata_;
}

Result<std::vector<std::uint8_t>> D88DiskContainer::ReadSector(const int cylinder, const int head, const int sector) const
{
    const auto it = std::find_if(
        sectors_.begin(),
        sectors_.end(),
        [&](const auto& block)
        {
            return block.sector.cylinder == cylinder &&
                   block.sector.head == head &&
                   block.sector.sector == sector;
        });
    if (it == sectors_.end())
    {
        return Result<std::vector<std::uint8_t>>::Failure(StatusCode::OutOfRange, "Sector not found.");
    }

    return Result<std::vector<std::uint8_t>>::Success(it->data);
}

Status D88DiskContainer::WriteSector(const int cylinder, const int head, const int sector, const std::span<const std::uint8_t> data)
{
    if (read_only_)
    {
        return {StatusCode::InvalidArgument, "D88 container is read-only."};
    }

    const auto it = std::find_if(
        sectors_.begin(),
        sectors_.end(),
        [&](const auto& block)
        {
            return block.sector.cylinder == cylinder &&
                   block.sector.head == head &&
                   block.sector.sector == sector;
        });
    if (it == sectors_.end())
    {
        return {StatusCode::OutOfRange, "Sector not found."};
    }

    it->sector.size = static_cast<int>(data.size());
    it->data.assign(data.begin(), data.end());
    return Status::OkStatus();
}

bool D88DiskContainer::SectorExists(const int cylinder, const int head, const int sector) const
{
    return std::any_of(
        sectors_.begin(),
        sectors_.end(),
        [&](const auto& block)
        {
            return block.sector.cylinder == cylinder &&
                   block.sector.head == head &&
                   block.sector.sector == sector;
        });
}

std::vector<SectorInfo> D88DiskContainer::GetAllSectors() const
{
    std::vector<SectorInfo> sectors;
    sectors.reserve(sectors_.size());
    for (const auto& block : sectors_)
    {
        sectors.push_back(block.sector);
    }
    return sectors;
}

std::vector<std::uint8_t> D88DiskContainer::ToImageData() const
{
    std::vector<std::uint8_t> image(0x2b0, 0x00);
    const auto copy_length = std::min<std::size_t>(image_name_.size(), 17);
    std::copy_n(image_name_.begin(), copy_length, image.begin());
    image[0x1a] = metadata_.is_write_protected ? 0x10 : 0x00;
    image[0x1b] = static_cast<std::uint8_t>(metadata_.disk_type);

    std::array<std::uint32_t, 164> track_offsets{};
    std::uint32_t current_offset = 0x2b0;
    std::size_t track_offset_position = 0x20;

    for (int cylinder = 0; cylinder < metadata_.geometry.cylinders; ++cylinder)
    {
        for (int head = 0; head < metadata_.geometry.heads; ++head)
        {
            std::vector<const SectorDataBlock*> track_sectors;
            for (const auto& sector : sectors_)
            {
                if (sector.sector.cylinder == cylinder && sector.sector.head == head)
                {
                    track_sectors.push_back(&sector);
                }
            }

            if (track_sectors.empty())
            {
                continue;
            }

            const auto track_index = static_cast<std::size_t>(cylinder * 2 + head);
            if (track_index < track_offsets.size())
            {
                track_offsets[track_index] = current_offset;
            }

            std::sort(
                track_sectors.begin(),
                track_sectors.end(),
                [](const auto* left, const auto* right)
                {
                    return left->sector.sector < right->sector.sector;
                });

            for (const auto* sector : track_sectors)
            {
                image.push_back(static_cast<std::uint8_t>(sector->sector.cylinder));
                image.push_back(static_cast<std::uint8_t>(sector->sector.head));
                image.push_back(static_cast<std::uint8_t>(sector->sector.sector));
                image.push_back(DetermineSectorSizeCode(sector->sector.size));
                image.push_back(static_cast<std::uint8_t>(track_sectors.size() & 0xff));
                image.push_back(static_cast<std::uint8_t>((track_sectors.size() >> 8) & 0xff));
                image.push_back(metadata_.disk_type == DiskType::TwoHD ? 0x01 : 0x00);
                image.push_back(sector->sector.is_deleted ? 0x10 : 0x00);
                image.push_back(sector->sector.has_error ? 0x01 : 0x00);
                image.insert(image.end(), 5, 0x00);
                image.push_back(static_cast<std::uint8_t>(sector->data.size() & 0xff));
                image.push_back(static_cast<std::uint8_t>((sector->data.size() >> 8) & 0xff));
                image.insert(image.end(), sector->data.begin(), sector->data.end());
                current_offset += static_cast<std::uint32_t>(16 + sector->data.size());
            }
        }
    }

    const auto disk_size = static_cast<std::uint32_t>(image.size());
    image[0x1c] = static_cast<std::uint8_t>(disk_size & 0xff);
    image[0x1d] = static_cast<std::uint8_t>((disk_size >> 8) & 0xff);
    image[0x1e] = static_cast<std::uint8_t>((disk_size >> 16) & 0xff);
    image[0x1f] = static_cast<std::uint8_t>((disk_size >> 24) & 0xff);

    for (const auto track_offset : track_offsets)
    {
        image[track_offset_position + 0] = static_cast<std::uint8_t>(track_offset & 0xff);
        image[track_offset_position + 1] = static_cast<std::uint8_t>((track_offset >> 8) & 0xff);
        image[track_offset_position + 2] = static_cast<std::uint8_t>((track_offset >> 16) & 0xff);
        image[track_offset_position + 3] = static_cast<std::uint8_t>((track_offset >> 24) & 0xff);
        track_offset_position += 4;
    }

    return image;
}
}
