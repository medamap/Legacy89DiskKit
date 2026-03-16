#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"

#include <array>
#include <cstdint>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateRawImage()
{
    std::vector<std::uint8_t> image(327680, 0x00);
    image[0] = 0x10;
    image[1] = 0x20;
    image[256] = 0x30;
    return image;
}
}

int main()
{
    auto read_only_result = RawDiskContainer::OpenFromBuffer(CreateRawImage(), true);
    if (!read_only_result.ok())
    {
        return 1;
    }

    const auto metadata = read_only_result.value().GetMetadata();
    if (metadata.disk_type != DiskType::TwoD || !metadata.is_write_protected)
    {
        return 2;
    }

    const auto sector0 = read_only_result.value().ReadSector(0, 0, 1);
    if (!sector0.ok() || sector0.value()[0] != 0x10 || sector0.value()[1] != 0x20)
    {
        return 3;
    }

    const auto sector1 = read_only_result.value().ReadSector(0, 0, 2);
    if (!sector1.ok() || sector1.value()[0] != 0x30)
    {
        return 4;
    }

    if (!read_only_result.value().SectorExists(39, 1, 16) || read_only_result.value().SectorExists(40, 0, 1))
    {
        return 5;
    }

    auto writable_result = RawDiskContainer::OpenFromBuffer(CreateRawImage(), false);
    if (!writable_result.ok())
    {
        return 6;
    }

    const std::array<std::uint8_t, 256> replacement{0xaa};
    const auto write_status = writable_result.value().WriteSector(0, 0, 1, replacement);
    if (!write_status.ok())
    {
        return 7;
    }

    const auto written_sector = writable_result.value().ReadSector(0, 0, 1);
    if (!written_sector.ok() || written_sector.value()[0] != 0xaa)
    {
        return 8;
    }

    const auto readonly_write = read_only_result.value().WriteSector(0, 0, 1, replacement);
    if (readonly_write.ok())
    {
        return 9;
    }

    if (writable_result.value().GetAllSectors().size() != 1280)
    {
        return 10;
    }

    if (writable_result.value().ToImageData()[0] != 0xaa)
    {
        return 11;
    }

    return 0;
}
