#include "legacy89diskkit/cpp/d88_parser.hpp"

#include <algorithm>
#include <cstring>

namespace legacy89diskkit::cpp
{
namespace
{
std::uint16_t ReadUInt16(const std::vector<std::uint8_t>& data, std::size_t offset)
{
    return static_cast<std::uint16_t>(data[offset] | (static_cast<std::uint16_t>(data[offset + 1]) << 8));
}

std::uint32_t ReadUInt32(const std::vector<std::uint8_t>& data, std::size_t offset)
{
    return static_cast<std::uint32_t>(data[offset]) |
           (static_cast<std::uint32_t>(data[offset + 1]) << 8) |
           (static_cast<std::uint32_t>(data[offset + 2]) << 16) |
           (static_cast<std::uint32_t>(data[offset + 3]) << 24);
}

DiskGeometryInfo CreateDefaultGeometry(DiskType disk_type)
{
    switch (disk_type)
    {
    case DiskType::TwoHD:
        return {77, 2, 26, 1024};
    case DiskType::TwoDD:
        return {40, 2, 16, 256};
    default:
        return {40, 2, 16, 256};
    }
}
}

Result<D88Header> D88Parser::ParseHeader(const std::vector<std::uint8_t>& image_data)
{
    if (image_data.size() < 0x2b0)
    {
        return Result<D88Header>::Failure(StatusCode::ParseError, "D88 image is too small.");
    }

    D88Header header{};
    header.image_name = std::string(reinterpret_cast<const char*>(image_data.data()), 17);
    if (const auto pos = header.image_name.find('\0'); pos != std::string::npos)
    {
        header.image_name.resize(pos);
    }

    header.write_protected = image_data[0x1a] != 0;
    const auto media_type = image_data[0x1b];
    switch (media_type)
    {
    case 0x00:
        header.media_type = DiskType::TwoD;
        break;
    case 0x10:
        header.media_type = DiskType::TwoDD;
        break;
    case 0x20:
        header.media_type = DiskType::TwoHD;
        break;
    case 0x80:
        header.media_type = DiskType::HardDisk;
        break;
    default:
        return Result<D88Header>::Failure(StatusCode::UnsupportedFormat, "Unsupported D88 media type.");
    }

    header.disk_size = ReadUInt32(image_data, 0x1c);
    for (std::size_t i = 0; i < header.track_offsets.size(); ++i)
    {
        header.track_offsets[i] = ReadUInt32(image_data, 0x20 + (i * 4));
    }

    return Result<D88Header>::Success(header);
}

Result<ReadOnlyDiskImageLayout> D88Parser::ParseImage(const std::vector<std::uint8_t>& image_data)
{
    const auto header_result = ParseHeader(image_data);
    if (!header_result.ok())
    {
        return Result<ReadOnlyDiskImageLayout>::Failure(header_result.status().code, header_result.status().message);
    }

    const auto& header = header_result.value();
    std::vector<SectorDataBlock> sectors;

    for (std::size_t track_index = 0; track_index < header.track_offsets.size(); ++track_index)
    {
        const auto track_offset = header.track_offsets[track_index];
        if (track_offset == 0 || track_offset >= image_data.size())
        {
            continue;
        }

        auto position = static_cast<std::size_t>(track_offset);
        std::uint16_t sectors_in_track = 0;
        std::uint16_t seen = 0;

        while (position + 16 <= image_data.size())
        {
            const auto cylinder = image_data[position + 0];
            const auto head = image_data[position + 1];
            const auto sector = image_data[position + 2];
            const auto sector_count = ReadUInt16(image_data, position + 4);
            const auto deleted = image_data[position + 7] != 0;
            const auto status = image_data[position + 8];
            const auto actual_size = ReadUInt16(image_data, position + 14);

            if (position + 16 + actual_size > image_data.size())
            {
                return Result<ReadOnlyDiskImageLayout>::Failure(StatusCode::ParseError, "D88 sector exceeds image bounds.");
            }

            sectors.push_back(SectorDataBlock{
                SectorInfo{
                    static_cast<int>(cylinder),
                    static_cast<int>(head),
                    static_cast<int>(sector),
                    static_cast<int>(actual_size),
                    deleted,
                    status != 0},
                std::vector<std::uint8_t>(image_data.begin() + static_cast<long>(position + 16),
                                          image_data.begin() + static_cast<long>(position + 16 + actual_size))});

            position += 16 + actual_size;
            if (sectors_in_track == 0)
            {
                sectors_in_track = sector_count;
            }
            ++seen;

            if (seen >= sectors_in_track)
            {
                break;
            }

            if (track_index + 1 < header.track_offsets.size() &&
                header.track_offsets[track_index + 1] != 0 &&
                position >= header.track_offsets[track_index + 1])
            {
                break;
            }
        }
    }

    DiskGeometryInfo geometry = CreateDefaultGeometry(header.media_type);
    if (!sectors.empty())
    {
        int max_cylinder = 0;
        int max_head = 0;
        int max_sector_size = 0;
        int max_sectors_per_track = 0;
        std::vector<std::pair<int, int>> track_keys;
        for (const auto& block : sectors)
        {
            max_cylinder = std::max(max_cylinder, block.sector.cylinder);
            max_head = std::max(max_head, block.sector.head);
            max_sector_size = std::max(max_sector_size, block.sector.size);
            track_keys.push_back({block.sector.cylinder, block.sector.head});
        }
        std::sort(track_keys.begin(), track_keys.end());
        auto current = std::pair<int, int>{-1, -1};
        int count = 0;
        for (const auto& key : track_keys)
        {
            if (key != current)
            {
                if (count > 0)
                {
                    max_sectors_per_track = std::max(max_sectors_per_track, count);
                }
                current = key;
                count = 1;
            }
            else
            {
                ++count;
            }
        }
        max_sectors_per_track = std::max(max_sectors_per_track, count);
        geometry = {max_cylinder + 1, max_head + 1, max_sectors_per_track, max_sector_size};
    }

    ReadOnlyDiskImageLayout layout{
        DiskContainerMetadata{"d88-sector-container", header.media_type, geometry, header.write_protected, header.disk_size},
        std::move(sectors)};
    return Result<ReadOnlyDiskImageLayout>::Success(std::move(layout));
}
}
