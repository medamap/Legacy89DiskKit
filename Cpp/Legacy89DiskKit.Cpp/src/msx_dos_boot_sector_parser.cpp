#include "legacy89diskkit/cpp/msx_dos_boot_sector_parser.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
namespace
{
std::uint16_t ReadUInt16(const std::vector<std::uint8_t>& bytes, const int offset)
{
    return static_cast<std::uint16_t>(bytes[offset] | (bytes[offset + 1] << 8));
}

void WriteUInt16(std::vector<std::uint8_t>& bytes, const int offset, const std::uint16_t value)
{
    bytes[offset] = static_cast<std::uint8_t>(value & 0xff);
    bytes[offset + 1] = static_cast<std::uint8_t>((value >> 8) & 0xff);
}
}

std::optional<MsxDosBootSector> MsxDosBootSectorParser::Parse(const std::vector<std::uint8_t>& sector_data)
{
    if (sector_data.size() < 512)
    {
        return std::nullopt;
    }

    MsxDosBootSector boot_sector{};
    std::copy_n(sector_data.begin(), 3, boot_sector.jump.begin());
    std::copy_n(sector_data.begin() + 3, 8, boot_sector.oem_name.begin());
    boot_sector.configuration = MsxDosConfiguration{
        ReadUInt16(sector_data, 0x0b),
        sector_data[0x0d],
        ReadUInt16(sector_data, 0x0e),
        sector_data[0x10],
        ReadUInt16(sector_data, 0x11),
        ReadUInt16(sector_data, 0x16),
        ReadUInt16(sector_data, 0x18),
        ReadUInt16(sector_data, 0x1a),
        ReadUInt16(sector_data, 0x13),
        sector_data[0x15] };
    return boot_sector;
}

std::vector<std::uint8_t> MsxDosBootSectorParser::Write(const MsxDosBootSector& boot_sector)
{
    std::vector<std::uint8_t> sector_data(512, 0x00);
    std::copy(boot_sector.jump.begin(), boot_sector.jump.end(), sector_data.begin());
    std::copy(boot_sector.oem_name.begin(), boot_sector.oem_name.end(), sector_data.begin() + 3);
    WriteUInt16(sector_data, 0x0b, static_cast<std::uint16_t>(boot_sector.configuration.sector_size));
    sector_data[0x0d] = static_cast<std::uint8_t>(boot_sector.configuration.sectors_per_cluster);
    WriteUInt16(sector_data, 0x0e, static_cast<std::uint16_t>(boot_sector.configuration.reserved_sectors));
    sector_data[0x10] = static_cast<std::uint8_t>(boot_sector.configuration.number_of_fats);
    WriteUInt16(sector_data, 0x11, static_cast<std::uint16_t>(boot_sector.configuration.root_directory_entries));
    WriteUInt16(sector_data, 0x13, static_cast<std::uint16_t>(boot_sector.configuration.total_sectors));
    sector_data[0x15] = boot_sector.configuration.media_descriptor;
    WriteUInt16(sector_data, 0x16, static_cast<std::uint16_t>(boot_sector.configuration.sectors_per_fat));
    WriteUInt16(sector_data, 0x18, static_cast<std::uint16_t>(boot_sector.configuration.sectors_per_track));
    WriteUInt16(sector_data, 0x1a, static_cast<std::uint16_t>(boot_sector.configuration.number_of_heads));
    sector_data[510] = 0x55;
    sector_data[511] = 0xaa;
    return sector_data;
}
}
