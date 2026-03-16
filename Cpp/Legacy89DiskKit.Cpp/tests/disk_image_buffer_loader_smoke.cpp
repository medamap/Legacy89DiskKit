#include "legacy89diskkit/cpp/infrastructure/disk_image/disk_image_buffer_loader.hpp"

#include <cstdint>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateMinimalD88()
{
    std::vector<std::uint8_t> image(0x2b0 + 17, 0x00);
    const char name[] = "BUFFER";
    for (std::size_t i = 0; i < sizeof(name) - 1; ++i)
    {
        image[i] = static_cast<std::uint8_t>(name[i]);
    }

    image[0x1b] = 0x00;
    const auto disk_size = static_cast<std::uint32_t>(image.size());
    image[0x1c] = static_cast<std::uint8_t>(disk_size & 0xff);
    image[0x1d] = static_cast<std::uint8_t>((disk_size >> 8) & 0xff);
    image[0x1e] = static_cast<std::uint8_t>((disk_size >> 16) & 0xff);
    image[0x1f] = static_cast<std::uint8_t>((disk_size >> 24) & 0xff);

    const std::uint32_t track0 = 0x2b0;
    image[0x20] = static_cast<std::uint8_t>(track0 & 0xff);
    image[0x21] = static_cast<std::uint8_t>((track0 >> 8) & 0xff);
    image[0x22] = static_cast<std::uint8_t>((track0 >> 16) & 0xff);
    image[0x23] = static_cast<std::uint8_t>((track0 >> 24) & 0xff);

    const std::size_t offset = 0x2b0;
    image[offset + 0] = 0;
    image[offset + 1] = 0;
    image[offset + 2] = 1;
    image[offset + 3] = 1;
    image[offset + 4] = 1;
    image[offset + 5] = 0;
    image[offset + 14] = 1;
    image[offset + 15] = 0;
    image[offset + 16] = 0x5a;
    return image;
}

std::vector<std::uint8_t> CreateMinimalRaw()
{
    std::vector<std::uint8_t> image(327680, 0x00);
    image[0] = 0x11;
    image[255] = 0x22;
    image[256] = 0x33;
    return image;
}
}

int main()
{
    const auto d88_result = DiskImageBufferLoader::Load(CreateMinimalD88(), ".d88");
    if (!d88_result.ok())
    {
        return 1;
    }

    if (d88_result.value().metadata.image_format != "d88-sector-container")
    {
        return 2;
    }

    if (d88_result.value().sectors.size() != 1 || d88_result.value().sectors[0].data[0] != 0x5a)
    {
        return 3;
    }

    const auto raw_result = DiskImageBufferLoader::Load(CreateMinimalRaw(), "2d");
    if (!raw_result.ok())
    {
        return 4;
    }

    if (raw_result.value().metadata.image_format != "raw-sector-container")
    {
        return 5;
    }

    if (raw_result.value().metadata.geometry.cylinders != 40 || raw_result.value().sectors.size() != 1280)
    {
        return 6;
    }

    if (raw_result.value().sectors[0].data[0] != 0x11 || raw_result.value().sectors[1].data[0] != 0x33)
    {
        return 7;
    }

    const auto unsupported = DiskImageBufferLoader::Load(CreateMinimalRaw(), ".xdf");
    if (unsupported.ok() || unsupported.status().code != StatusCode::UnsupportedFormat)
    {
        return 8;
    }

    return 0;
}
