#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/msx_dos_boot_sector_parser.hpp"
#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"
#include "legacy89diskkit/cpp/msx_dos_default_attribute_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_dir_parser.hpp"
#include "legacy89diskkit/cpp/msx_dos_directory_listing.hpp"
#include "legacy89diskkit/cpp/msx_dos_fat_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_file_lookup.hpp"
#include "legacy89diskkit/cpp/msx_dos_mode_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_read_rules.hpp"
#include "legacy89diskkit/cpp/msx_dos_shell.hpp"

#include <algorithm>
#include <array>
#include <vector>

using namespace legacy89diskkit::cpp;

int main()
{
    const auto config = MsxDosConfigurationProvider::GetDefault(DiskType::TwoDD);
    if (config.ClusterSize() != 1024 || config.RootDirectorySectors() != 7 || config.FirstDataSector() != 26)
    {
        return 1;
    }

    MsxDosBootSector boot_sector{
        { 0xeb, 0xfe, 0x90 },
        { 'M', 'S', 'X', '-', 'D', 'O', 'S', ' ' },
        config };
    const auto boot_bytes = MsxDosBootSectorParser::Write(boot_sector);
    const auto parsed_boot = MsxDosBootSectorParser::Parse(boot_bytes);
    if (!parsed_boot.has_value() ||
        parsed_boot->configuration.sector_size != 512 ||
        parsed_boot->configuration.media_descriptor != 0xf9)
    {
        return 2;
    }

    std::array<std::uint8_t, 32> entry{};
    entry.fill(static_cast<std::uint8_t>(' '));
    entry[0] = 'M';
    entry[1] = 'S';
    entry[2] = 'X';
    entry[8] = 'B';
    entry[9] = 'A';
    entry[10] = 'S';
    entry[11] = 0x20;
    entry[26] = 0x02;
    entry[28] = 0x34;
    entry[29] = 0x12;
    const auto parsed_entry = MsxDosDirParser::ParseFileEntry(entry);
    if (parsed_entry.file_name != "MSX" || parsed_entry.extension != "BAS" || parsed_entry.size != 0x1234)
    {
        return 3;
    }

    const auto written_entry = MsxDosDirParser::Write(parsed_entry);
    if (written_entry[0] != 'M' || written_entry[8] != 'B' || written_entry[11] != 0x20)
    {
        return 4;
    }

    std::vector<std::uint8_t> fat(64, 0x00);
    MsxDosFatRules::SetEntry(fat, 2, 3);
    MsxDosFatRules::SetEntry(fat, 3, 0xfff);
    const auto chain = MsxDosFatRules::GetClusterChain(fat, config, 2);
    if (chain.size() != 2 || chain[0] != 2 || chain[1] != 3)
    {
        return 5;
    }

    const auto resolved_size = MsxDosReadRules::ResolveSizeFromFat(chain, config, 1500);
    const auto ascii_payload = MsxDosReadRules::ResolveReadPayload(
        { 'A', 'B', 0x1a, 'C' },
        MsxDosFileEntry{
            "MSX", "TXT", 4, MsxDosFileAttributes{ true, 0x00, false, false, false, false, false }, 2, 0, 0, {}, {} });
    if (resolved_size != 1500 || ascii_payload.size() != 2)
    {
        return 6;
    }

    std::vector<std::vector<std::uint8_t>> directory(1, std::vector<std::uint8_t>(config.sector_size, 0x00));
    std::copy(entry.begin(), entry.end(), directory[0].begin());
    directory[0][32] = 0x00;
    const auto files = MsxDosDirectoryListing::ListFiles(directory, fat, config);
    if (files.size() != 1 || files[0].file_name != "MSX")
    {
        return 7;
    }

    const auto default_attributes = MsxDosDefaultAttributeRules::CreateDefaultAttributes(false);
    if (!default_attributes.is_ascii || default_attributes.raw_attributes != 0x00)
    {
        return 8;
    }

    const auto parsed_attributes = MsxDosModeRules::Parse(0x23);
    if (!parsed_attributes.is_read_only || !parsed_attributes.is_hidden || !parsed_attributes.is_archive)
    {
        return 9;
    }

    if (!MsxDosShell::FileExists(directory, fat, config, "MSX.BAS") ||
        !MsxDosShell::FindFile(directory, fat, config, "MSX.BAS").has_value())
    {
        return 10;
    }

    const auto info = MsxDosShell::GetFileSystemInfo(fat, config);
    if (info.cluster_size != config.ClusterSize() || info.first_data_sector != config.FirstDataSector())
    {
        return 11;
    }

    return 0;
}
