#include "legacy89diskkit/cpp/msx_dos_format_rules.hpp"

#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"

namespace legacy89diskkit::cpp
{
std::vector<std::uint8_t> MsxDosFormatRules::CreateFatData(const MsxDosConfiguration& config)
{
    std::vector<std::uint8_t> fat_data(config.sectors_per_fat * config.sector_size, 0x00);
    MsxDosFatRules::SetEntry(fat_data, 0, static_cast<std::uint16_t>(0xf00 | config.media_descriptor));
    MsxDosFatRules::SetEntry(fat_data, 1, 0xfff);
    return fat_data;
}

std::vector<std::vector<std::uint8_t>> MsxDosFormatRules::CreateRootDirectorySectors(const MsxDosConfiguration& config)
{
    return std::vector<std::vector<std::uint8_t>>(
        config.RootDirectorySectors(),
        std::vector<std::uint8_t>(config.sector_size, 0x00));
}
}
