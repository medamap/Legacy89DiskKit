#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/drive/mounted_medium_binding_factory.hpp"

#include <array>
#include <cstdint>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateRawImage()
{
    std::vector<std::uint8_t> image(327680, 0x00);
    image[0] = 0x6c;
    image[1] = 0x7d;
    image[256] = 0x55;
    return image;
}

std::vector<std::uint8_t> CreateMinimalD88()
{
    std::vector<std::uint8_t> image(0x2b0 + 17, 0x00);
    const char name[] = "D88TEST";
    for (std::size_t i = 0; i < sizeof(name) - 1; ++i)
    {
        image[i] = static_cast<std::uint8_t>(name[i]);
    }

    const auto disk_size = static_cast<std::uint32_t>(image.size());
    image[0x1c] = static_cast<std::uint8_t>(disk_size & 0xff);
    image[0x1d] = static_cast<std::uint8_t>((disk_size >> 8) & 0xff);
    image[0x1e] = static_cast<std::uint8_t>((disk_size >> 16) & 0xff);
    image[0x1f] = static_cast<std::uint8_t>((disk_size >> 24) & 0xff);
    image[0x20] = 0xb0;
    image[0x21] = 0x02;

    const std::size_t offset = 0x2b0;
    image[offset + 2] = 1;
    image[offset + 3] = 1;
    image[offset + 4] = 1;
    image[offset + 14] = 1;
    image[offset + 16] = 0x6c;
    return image;
}
}

int main()
{
    auto raw_container_result = RawDiskContainer::OpenFromBuffer(CreateRawImage(), false);
    if (!raw_container_result.ok())
    {
        return 1;
    }

    auto raw_binding = MountedMediumBindingFactory::Create(raw_container_result.value());
    if (raw_binding.mounted_medium->MediumKind() != "raw-sector-image" ||
        !raw_binding.sector_medium->SectorExists(0, 0, 1))
    {
        return 2;
    }

    const auto raw_sector = raw_binding.sector_medium->ReadSector(0, 0, 1);
    if (raw_sector.size() < 2 || raw_sector[0] != 0x6c || raw_sector[1] != 0x7d)
    {
        return 3;
    }

    raw_binding.controller_facing_medium->WriteTrackRegister(0);
    raw_binding.controller_facing_medium->WriteSectorRegister(1);
    raw_binding.controller_facing_medium->WriteCommand(0x80);
    raw_binding.controller_facing_medium->Advance(std::chrono::milliseconds(1));
    if (raw_binding.controller_facing_medium->ReadDataRegister() != 0x6c)
    {
        return 4;
    }

    auto d88_container_result = D88DiskContainer::OpenFromBuffer(CreateMinimalD88(), false);
    if (!d88_container_result.ok())
    {
        return 5;
    }

    auto d88_binding = MountedMediumBindingFactory::Create(d88_container_result.value());
    if (d88_binding.mounted_medium->MediumKind() != "d88-family" ||
        !d88_binding.sector_medium->SectorExists(0, 0, 1))
    {
        return 6;
    }

    const auto d88_sector = d88_binding.sector_medium->ReadSector(0, 0, 1);
    if (d88_sector.empty() || d88_sector[0] != 0x6c)
    {
        return 7;
    }

    d88_binding.controller_facing_medium->WriteTrackRegister(0);
    d88_binding.controller_facing_medium->WriteSectorRegister(1);
    d88_binding.controller_facing_medium->WriteCommand(0x80);
    d88_binding.controller_facing_medium->Advance(std::chrono::milliseconds(1));
    if (d88_binding.controller_facing_medium->ReadDataRegister() != 0x6c)
    {
        return 8;
    }

    return 0;
}
