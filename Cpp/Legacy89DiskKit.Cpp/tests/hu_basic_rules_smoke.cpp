#include "legacy89diskkit/cpp/hu_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_rules.hpp"
#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_configuration.hpp"
#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_mode_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_read_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_write_rules.hpp"

#include <algorithm>
#include <cstdint>
#include <vector>

using namespace legacy89diskkit::cpp;

int main()
{
    const auto parsed = HuBasicNameRules::ParseFileName("LONGFILENAME12345.EXTENDED");
    if (parsed.file_name != "LONGFILENAME1" || parsed.extension != "EXT")
    {
        return 1;
    }

    if (HuBasicNameRules::BuildDisplayName("HELLO", "BAS") != "HELLO.BAS")
    {
        return 2;
    }

    const auto config = HuBasicConfigurationProvider::GetDefault(DiskType::TwoD);
    std::vector<std::uint8_t> fat(256, 0x00);
    HuBasicFatRules::ApplyChain(fat, { 0x10, 0x11 }, 0x83);
    const auto chain = HuBasicFatRules::GetClusterChain(fat, config, 0x10);
    if (chain.chain.size() != 2 || chain.terminal_flag != 0x83)
    {
        return 3;
    }

    const auto two_hd_config = HuBasicConfigurationProvider::GetDefault(DiskType::TwoHD);
    if (HuBasicAllocationRules::IsAllocatableCluster(DiskType::TwoHD, two_hd_config, 0x80) ||
        !HuBasicAllocationRules::IsAllocatableCluster(DiskType::TwoHD, two_hd_config, 0x100))
    {
        return 4;
    }

    std::vector<std::uint8_t> free_fat(512, 0x00);
    HuBasicFatRules::SetEntry(free_fat, two_hd_config.reserved_clusters, 0x8f);
    const auto free_clusters = HuBasicAllocationRules::CollectFreeClusters(free_fat, DiskType::TwoHD, two_hd_config, 4);
    if (free_clusters.size() != 4 || std::find(free_clusters.begin(), free_clusters.end(), 0x80) != free_clusters.end())
    {
        return 5;
    }

    HuBasicFileEntry binary_file{
        "BIN", "DAT", 3, HuBasicFileAttributes{ false, 0x01, false, false, false }, 0, 0, 0, 0,
        HuBasicFileMetadata{ HuBasicFileType::Binary, false, false, false, false, false, 3, 0, 0, 0, 0x01, 0x20 } };
    const auto trimmed = HuBasicReadRules::ResolveReadPayload({ 0x10, 0x20, 0x30, 0x40 }, binary_file, DiskType::TwoD, config, 1, 0xff);
    if (trimmed.size() != 3 || trimmed[2] != 0x30)
    {
        return 6;
    }

    HuBasicFileEntry ascii_file{
        "ASC", "TXT", 0, HuBasicFileAttributes{ true, 0x04, false, false, false }, 0, 0, 0, 0,
        HuBasicFileMetadata{ HuBasicFileType::Ascii, false, false, false, false, false, 0, 0, 0, 0, 0x04, 0x20 } };
    const auto ascii = HuBasicReadRules::ResolveReadPayload({ 'A', 'B', 0x1a, 'C' }, ascii_file, DiskType::TwoD, config, 1, 0xff);
    if (ascii.size() != 2 || ascii[0] != 'A' || ascii[1] != 'B')
    {
        return 7;
    }

    HuBasicFileEntry two_hd_file{
        "HD", "BIN", 0, HuBasicFileAttributes{ false, 0x01, false, false, false }, 0, 0, 0, 0,
        HuBasicFileMetadata{ HuBasicFileType::Binary, false, false, false, false, false, 0, 0, 0, 0, 0x01, 0x20 } };
    std::vector<std::uint8_t> two_hd_data(700, 0x5a);
    const auto two_hd = HuBasicReadRules::ResolveReadPayload(
        two_hd_data,
        two_hd_file,
        DiskType::TwoHD,
        two_hd_config,
        1,
        0x82);
    if (two_hd.size() != 768)
    {
        return 8;
    }

    const auto prepared_ascii = HuBasicWriteRules::PrepareWritePayload({ 'A', 'B' }, HuBasicFileAttributes{ true, 0x04, false, false, false });
    if (prepared_ascii.size() != 3 || prepared_ascii.back() != 0x1a)
    {
        return 9;
    }

    if (HuBasicWriteRules::GetClustersNeeded(config.cluster_size + 1, config) != 2 ||
        HuBasicWriteRules::GetTerminalFlagForLength(config.cluster_size + (3 * config.sector_size), config) != 0x82)
    {
        return 10;
    }

    const auto created = HuBasicDirectoryRules::CreateFileEntryForWrite(
        "ABCDEFGHIJKLMN.BINARY",
        std::vector<std::uint8_t>(16, 0x20),
        HuBasicFileAttributes{ false, 0x01, false, false, false },
        5,
        0x1000,
        0x1200);
    if (created.file_name != "ABCDEFGHIJKLM" || created.extension != "BIN" || created.start_cluster != 5 || created.end_address != 0x100f)
    {
        return 11;
    }

    if (HuBasicModeRules::GetFileType(0x01) != HuBasicFileType::Binary ||
        HuBasicModeRules::BuildModeByte(created.metadata) != 0x01 ||
        HuBasicModeRules::BuildModeByte(HuBasicFileAttributes{ true, 0x00, false, true, true }) != 0x54)
    {
        return 12;
    }

    std::array<std::uint8_t, 32> entry_bytes{};
    entry_bytes.fill(static_cast<std::uint8_t>(' '));
    entry_bytes[0] = 0x01;
    entry_bytes[1] = 'H';
    entry_bytes[2] = 'E';
    entry_bytes[3] = 'L';
    entry_bytes[4] = 'L';
    entry_bytes[5] = 'O';
    entry_bytes[0x0e] = 'B';
    entry_bytes[0x0f] = 'A';
    entry_bytes[0x10] = 'S';
    entry_bytes[0x11] = 0x20;
    entry_bytes[0x12] = 0x34;
    entry_bytes[0x13] = 0x12;
    entry_bytes[0x1e] = 0x21;
    entry_bytes[0x1f] = 0x01;
    const auto entry = HuBasicDirectoryEntryCodec::Parse(entry_bytes);
    if (entry.file_name != "HELLO" || entry.extension != "BAS" || entry.recorded_size != 0x1234)
    {
        return 13;
    }

    const auto roundtrip = HuBasicDirectoryEntryCodec::Write(entry);
    if (roundtrip[1] != 'H' || roundtrip[0x0e] != 'B' || roundtrip[0x12] != 0x34 || roundtrip[0x13] != 0x12)
    {
        return 14;
    }

    const auto parsed_entry = HuBasicDirParser::Parse(entry);
    if (parsed_entry.file_name != "HELLO" ||
        parsed_entry.extension != "BAS" ||
        parsed_entry.size != 0x1234 ||
        parsed_entry.attributes.is_ascii ||
        parsed_entry.metadata.file_type != HuBasicFileType::Binary ||
        parsed_entry.metadata.password_byte != 0x20)
    {
        return 15;
    }

    return 0;
}
