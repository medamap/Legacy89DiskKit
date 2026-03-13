#include "legacy89diskkit/cpp/character_encoding.hpp"
#include "legacy89diskkit/cpp/d88_parser.hpp"
#include "legacy89diskkit/cpp/raw_disk_geometry.hpp"

#include <cstdint>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateMinimalD88()
{
    std::vector<std::uint8_t> image(0x2b0 + 17, 0x00);
    const char name[] = "SMOKE";
    for (std::size_t i = 0; i < sizeof(name) - 1; ++i)
    {
        image[i] = static_cast<std::uint8_t>(name[i]);
    }
    image[0x1b] = 0x00;
    const std::uint32_t disk_size = static_cast<std::uint32_t>(image.size());
    image[0x1c] = static_cast<std::uint8_t>(disk_size & 0xff);
    image[0x1d] = static_cast<std::uint8_t>((disk_size >> 8) & 0xff);
    image[0x1e] = static_cast<std::uint8_t>((disk_size >> 16) & 0xff);
    image[0x1f] = static_cast<std::uint8_t>((disk_size >> 24) & 0xff);
    const std::uint32_t track0 = 0x2b0;
    image[0x20] = static_cast<std::uint8_t>(track0 & 0xff);
    image[0x21] = static_cast<std::uint8_t>((track0 >> 8) & 0xff);
    image[0x22] = static_cast<std::uint8_t>((track0 >> 16) & 0xff);
    image[0x23] = static_cast<std::uint8_t>((track0 >> 24) & 0xff);

    const std::size_t o = 0x2b0;
    image[o + 0] = 0;
    image[o + 1] = 0;
    image[o + 2] = 1;
    image[o + 3] = 1;
    image[o + 4] = 1;
    image[o + 5] = 0;
    image[o + 14] = 1;
    image[o + 15] = 0;
    image[o + 16] = 0xaa;
    return image;
}
}

int main()
{
    const auto geometry = RawDiskGeometryDetector::Detect(327680);
    if (geometry.cylinders != 40 || geometry.sides != 2)
    {
        return 1;
    }

    const RawSectorAddressCalculator calculator(geometry);
    const auto offset = calculator.CalculateOffset(0, 0, 1);
    if (!offset.ok() || offset.value() != 0)
    {
        return 2;
    }

    const auto d88 = D88Parser::ParseImage(CreateMinimalD88());
    if (!d88.ok())
    {
        return 3;
    }
    if (d88.value().sectors.size() != 1)
    {
        return 4;
    }
    if (d88.value().sectors[0].data[0] != 0xaa)
    {
        return 5;
    }

    const auto profile = CharacterEncodingResolver::ResolveProfile("", "x1", "");
    if (!profile.ok() || profile.value().machine_type != "X1")
    {
        return 6;
    }

    return 0;
}
