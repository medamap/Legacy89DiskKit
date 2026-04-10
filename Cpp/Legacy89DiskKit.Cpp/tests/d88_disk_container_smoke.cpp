#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/d88_parser.hpp"

#include <array>
#include <cstdint>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateMinimalD88()
{
    std::vector<std::uint8_t> image(0x2b0 + 17, 0x00);
    const char name[] = "D88TEST";
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
    image[offset + 16] = 0x5c;
    return image;
}
}

int main()
{
    auto read_only_result = D88DiskContainer::OpenFromBuffer(CreateMinimalD88(), true);
    if (!read_only_result.ok())
    {
        return 1;
    }

    const auto metadata = read_only_result.value().GetMetadata();
    if (metadata.image_format != "d88-sector-container" || !metadata.is_write_protected)
    {
        return 2;
    }

    const auto sector = read_only_result.value().ReadSector(0, 0, 1);
    if (!sector.ok() || sector.value()[0] != 0x5c)
    {
        return 3;
    }

    if (!read_only_result.value().SectorExists(0, 0, 1) || read_only_result.value().SectorExists(0, 0, 2))
    {
        return 4;
    }

    auto writable_result = D88DiskContainer::OpenFromBuffer(CreateMinimalD88(), false);
    if (!writable_result.ok())
    {
        return 5;
    }

    const std::array<std::uint8_t, 1> replacement{0xa5};
    const auto write_status = writable_result.value().WriteSector(0, 0, 1, replacement);
    if (!write_status.ok())
    {
        return 6;
    }

    const auto image_after_write = writable_result.value().ToImageData();
    const auto reparsed = D88Parser::ParseImage(image_after_write);
    if (!reparsed.ok() || reparsed.value().sectors.size() != 1 || reparsed.value().sectors[0].data[0] != 0xa5)
    {
        return 7;
    }

    const auto readonly_write = read_only_result.value().WriteSector(0, 0, 1, replacement);
    if (readonly_write.ok())
    {
        return 8;
    }

    if (writable_result.value().GetAllSectors().size() != 1)
    {
        return 9;
    }

    return 0;
}
