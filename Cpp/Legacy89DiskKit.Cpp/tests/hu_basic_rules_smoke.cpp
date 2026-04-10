#include "legacy89diskkit/cpp/hu_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_attribute_update_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_boot_record_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_boot_record_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_cluster_write_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_default_attribute_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_delete_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_layout_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_sector_rules.hpp"
#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_format_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_configuration.hpp"
#include "legacy89diskkit/cpp/hu_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_file_entry_writer.hpp"
#include "legacy89diskkit/cpp/hu_basic_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_label_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_mode_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_read_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_record_address_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_rename_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_shell.hpp"
#include "legacy89diskkit/cpp/hu_basic_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_virtual_label_entry_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_write_transaction.hpp"
#include "legacy89diskkit/cpp/hu_basic_write_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_configuration.hpp"
#include "legacy89diskkit/cpp/n88_basic_allocation_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_attribute_update_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_default_attribute_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_delete_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_dir_parser.hpp"
#include "legacy89diskkit/cpp/n88_basic_directory_listing.hpp"
#include "legacy89diskkit/cpp/n88_basic_fat_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_format_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_file_entry_writer.hpp"
#include "legacy89diskkit/cpp/n88_basic_read_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_file_lookup.hpp"
#include "legacy89diskkit/cpp/n88_basic_filesystem_info_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_rename_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_shell.hpp"
#include "legacy89diskkit/cpp/n88_basic_types.hpp"
#include "legacy89diskkit/cpp/n88_basic_write_rules.hpp"
#include "legacy89diskkit/cpp/n88_basic_write_transaction.hpp"

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

    std::vector<std::uint8_t> boot_area(32, static_cast<std::uint8_t>(' '));
    boot_area[0] = 0x01;
    boot_area[1] = 'S';
    boot_area[2] = 'Y';
    boot_area[3] = 'S';
    boot_area[0x0e] = 'B';
    boot_area[0x0f] = 'I';
    boot_area[0x10] = 'N';
    boot_area[0x11] = 0x21;
    boot_area[0x12] = 0x34;
    boot_area[0x13] = 0x12;
    boot_area[0x14] = 0x00;
    boot_area[0x15] = 0x40;
    boot_area[0x16] = 0x00;
    boot_area[0x17] = 0x50;
    boot_area[0x1e] = 0x08;
    boot_area[0x1f] = 0x00;
    const auto boot_record = HuBasicBootRecordParser::Parse(boot_area);
    if (!boot_record.has_value() || boot_record->file_name != "SYS" || boot_record->extension != "BIN" || !boot_record->has_password)
    {
        return 16;
    }

    HuBasicFileEntry label_entry{
        "------", "", 0, HuBasicFileAttributes{ true, 0x44, false, true, false }, 0x7fff, 0xffff, 0xffff, 0xffff,
        HuBasicFileMetadata{ HuBasicFileType::Ascii, true, false, false, true, false, 0, 0xffff, 0xffff, 0x7fff, 0x44, 0x21 } };
    HuBasicFileEntry label_extension{
        ".TXT", "", 0, HuBasicFileAttributes{ true, 0x44, false, true, false }, 0x7fff, 0xffff, 0xffff, 0xffff,
        HuBasicFileMetadata{ HuBasicFileType::Ascii, true, false, false, true, false, 0, 0xffff, 0xffff, 0x7fff, 0x44, 0x21 } };
    if (!HuBasicLabelRules::IsVirtualLabelEntry(label_entry) ||
        !HuBasicLabelRules::CanMergeLabelEntries(label_entry, label_extension))
    {
        return 17;
    }

    std::vector<std::uint8_t> directory_sector(config.sector_size, 0x00);
    std::copy(roundtrip.begin(), roundtrip.end(), directory_sector.begin());
    directory_sector[32] = 0xff;
    if (HuBasicDirectorySectorRules::CountActiveEntries(directory_sector, config.sector_size) != 1 ||
        !HuBasicDirectorySectorRules::FindWritableSlotOffset(directory_sector, config.sector_size).has_value() ||
        HuBasicDirectorySectorRules::FindEntryOffset(directory_sector, config.sector_size, "HELLO.BAS") != 0)
    {
        return 18;
    }

    HuBasicDirectorySectorRules::MarkEntryDeleted(directory_sector, 0);
    if (directory_sector[0] != 0x00)
    {
        return 19;
    }

    std::vector<std::uint8_t> transaction_fat(256, 0x00);
    const auto plan = HuBasicWriteTransaction::CreatePlan(
        "HELLO.BAS",
        { 'P', 'R', 'I', 'N', 'T' },
        HuBasicFileAttributes{ true, 0x04, false, false, false },
        DiskType::TwoD,
        config,
        transaction_fat,
        0x2000,
        0x2100);
    if (!plan.has_value() ||
        plan->allocated_clusters.empty() ||
        plan->payload.back() != 0x1a ||
        plan->directory_entry.file_name != "HELLO" ||
        plan->file_entry.metadata.file_type != HuBasicFileType::Ascii)
    {
        return 20;
    }

    const auto written_entry = HuBasicFileEntryWriter::ToDirectoryEntry(plan->file_entry);
    if (written_entry.file_name != "HELLO" || written_entry.extension != "BAS" || written_entry.mode_byte != 0x04)
    {
        return 21;
    }

    const auto directory_layout = HuBasicDirectoryLayoutRules::BuildDirectorySectors({ plan->file_entry }, config.sector_size, 2);
    if (directory_layout.size() != 2 || directory_layout[0][0] != 0x04 || directory_layout[0][32] != 0xff)
    {
        return 22;
    }

    std::vector<std::uint8_t> info_fat(256, 0x00);
    HuBasicFatRules::SetEntry(info_fat, config.reserved_clusters, 0x8f);
    const auto info = HuBasicFileSystemInfoRules::BuildInfo(info_fat, DiskType::TwoD, config);
    if (info.total_size <= 0 || info.free_space <= 0 || info.cluster_size != config.cluster_size)
    {
        return 23;
    }

    const auto boot_roundtrip = HuBasicBootRecordCodec::Write(*boot_record);
    if (boot_roundtrip[0] != 0x01 || boot_roundtrip[1] != 'S' || boot_roundtrip[0x1e] != 0x08)
    {
        return 24;
    }

    const auto default_ascii = HuBasicDefaultAttributeRules::CreateDefaultAttributes(true);
    if (!default_ascii.is_ascii || default_ascii.raw_attributes != 0x04)
    {
        return 25;
    }

    auto renamed = HuBasicRenameRules::Rename(plan->file_entry, "RENAMED.BIN");
    if (renamed.file_name != "RENAMED" || renamed.extension != "BIN")
    {
        return 26;
    }

    renamed = HuBasicAttributeUpdateRules::UpdateAttributes(
        renamed,
        HuBasicFileAttributes{ false, 0x41, false, true, false });
    if (renamed.metadata.raw_mode_byte != 0x41 || renamed.metadata.is_write_protected != true)
    {
        return 27;
    }

    auto deleted_fat = fat;
    HuBasicDeleteRules::FreeClusters(deleted_fat, { 0x10, 0x11 });
    if (HuBasicFatRules::GetEntry(deleted_fat, 0x10) != 0x00 || HuBasicFatRules::GetEntry(deleted_fat, 0x11) != 0x00)
    {
        return 28;
    }

    const auto formatted_fat = HuBasicFormatRules::CreateFatData(config);
    const auto formatted_directory = HuBasicFormatRules::CreateDirectorySectors(config);
    if (HuBasicFatRules::GetEntry(formatted_fat, 0) != 0x01 ||
        HuBasicFatRules::GetEntry(formatted_fat, config.reserved_clusters - 1) != 0x8f ||
        formatted_directory.size() != static_cast<std::size_t>(config.directory_sectors) ||
        formatted_directory[0][0] != 0xff)
    {
        return 29;
    }

    const auto physical = HuBasicRecordAddressRules::GetPhysicalAddressFromRecord(17, config);
    if (physical.cylinder != 0 || physical.head != 1 || physical.sector != 2)
    {
        return 30;
    }

    const auto cluster_buffers = HuBasicClusterWriteRules::SplitIntoClusterBuffers(
        { 1, 2, 3, 4, 5 },
        { 4, 5 },
        config);
    if (cluster_buffers.size() != 2 || cluster_buffers[0][0] != 1 || cluster_buffers[0][4] != 5 || cluster_buffers[1][0] != 0)
    {
        return 31;
    }

    const auto virtual_label = HuBasicVirtualLabelEntryRules::CreateEntry(
        "TITLE",
        "",
        0x44,
        0x21,
        0,
        0xffff,
        0xffff,
        0xffff,
        0x7fff);
    if (!HuBasicLabelRules::IsVirtualLabelEntry(virtual_label))
    {
        return 32;
    }

    const auto shell_files = HuBasicShell::ListFiles({ directory_layout[0] }, config.sector_size);
    const auto shell_found = HuBasicShell::FindFile({ directory_layout[0] }, config.sector_size, "HELLO.BAS");
    const auto shell_layout = HuBasicShell::ReadDirectoryLayout({ directory_layout[0] }, config.sector_size);
    const auto shell_info = HuBasicShell::GetFileSystemInfo(info_fat, DiskType::TwoD, config);
    const auto shell_write = HuBasicShell::PlanWrite(
        "SHELL.BAS",
        { '1', '0' },
        HuBasicFileAttributes{ true, 0x04, false, false, false },
        DiskType::TwoD,
        config,
        transaction_fat,
        0x2200,
        0x2300);
    const auto shell_rename = HuBasicShell::PlanRename({ directory_layout[0] }, config.sector_size, "HELLO.BAS", "WORLD.BAS");
    const auto shell_update = HuBasicShell::PlanAttributeUpdate(
        { directory_layout[0] },
        config.sector_size,
        "HELLO.BAS",
        HuBasicFileAttributes{ false, 0x41, false, true, false });
    const auto shell_delete = HuBasicShell::PlanDelete(
        fat,
        { 0x10, 0x11 },
        { directory_layout[0] },
        config.sector_size,
        "HELLO.BAS");
    if (shell_files.size() != 1 ||
        !HuBasicShell::FileExists({ directory_layout[0] }, config.sector_size, "HELLO.BAS") ||
        !shell_found.has_value() ||
        shell_layout.items.size() != 1 ||
        shell_info.cluster_size != config.cluster_size ||
        !shell_write.has_value() ||
        !shell_rename.has_value() ||
        !shell_update.has_value() ||
        !shell_delete.has_value())
    {
        return 33;
    }

    const auto n88_config = N88BasicConfigurationProvider::GetDefault(DiskType::TwoD);
    std::array<std::uint8_t, 16> n88_entry{};
    n88_entry.fill(static_cast<std::uint8_t>(' '));
    n88_entry[0] = 'T';
    n88_entry[1] = 'E';
    n88_entry[2] = 'S';
    n88_entry[3] = 'T';
    n88_entry[6] = 'B';
    n88_entry[7] = 'A';
    n88_entry[8] = 'S';
    n88_entry[9] = 0x00;
    n88_entry[10] = 0x05;
    const auto n88_parsed = N88BasicDirParser::ParseFileEntry(n88_entry);
    if (n88_parsed.file_name != "TEST" || n88_parsed.extension != "BAS" || !n88_parsed.attributes.is_ascii)
    {
        return 34;
    }

    std::vector<std::uint8_t> n88_fat(256, 0xff);
    N88BasicFatRules::SetEntry(n88_fat, 0x05, 0xc2);
    const auto n88_chain = N88BasicFatRules::GetClusterChain(n88_fat, n88_config, 0x05);
    if (n88_chain.size() != 1 || N88BasicReadRules::ResolveSizeFromFat(n88_chain, n88_fat, n88_config) != 512)
    {
        return 35;
    }

    const auto n88_payload = N88BasicReadRules::ResolveReadPayload(
        { 'A', 'B', 0x1a, 'C' },
        N88BasicFileEntry{ "TEST", "BAS", 4, N88BasicFileAttributes{ true, 0x00, false }, 0x05 });
    if (n88_payload.size() != 2 || n88_payload[0] != 'A' || n88_payload[1] != 'B')
    {
        return 36;
    }

    const auto n88_default_attributes = N88BasicDefaultAttributeRules::CreateDefaultAttributes(false);
    if (n88_default_attributes.raw_attributes != 0x01 || n88_default_attributes.is_ascii)
    {
        return 37;
    }

    std::vector<std::vector<std::uint8_t>> n88_directory(1, std::vector<std::uint8_t>(n88_config.sector_size, 0xff));
    std::copy(n88_entry.begin(), n88_entry.end(), n88_directory[0].begin());
    const auto n88_files = N88BasicDirectoryListing::ListFiles(n88_directory, n88_fat, n88_config);
    if (n88_files.size() != 1 || n88_files[0].size != 512 || !N88BasicShell::FileExists(n88_directory, n88_fat, n88_config, "TEST.BAS"))
    {
        return 38;
    }

    const auto n88_written = N88BasicWriteRules::PrepareWritePayload(
        { 'A', 'B' },
        N88BasicFileAttributes{ true, 0x00, false });
    if (n88_written.size() != 3 || n88_written.back() != 0x1a)
    {
        return 39;
    }

    if (N88BasicWriteRules::GetClustersNeeded(n88_config.cluster_size + 1, n88_config) != 2 ||
        N88BasicWriteRules::GetTerminalFlagForLength(n88_config.cluster_size + 1, n88_config) != 0xc1)
    {
        return 40;
    }

    std::vector<std::uint8_t> n88_free_fat(256, 0x00);
    n88_free_fat[0] = 0xff;
    const auto n88_allocated = N88BasicAllocationRules::CollectFreeClusters(n88_free_fat, n88_config, 3);
    if (n88_allocated.size() != 3 || n88_allocated[0] != 1)
    {
        return 41;
    }

    auto n88_renamed = N88BasicRenameRules::Rename(
        N88BasicFileEntry{ "TEST", "BAS", 512, N88BasicFileAttributes{ true, 0x00, false }, 0x05 },
        "DATA.BIN");
    if (n88_renamed.file_name != "DATA" || n88_renamed.extension != "BIN")
    {
        return 42;
    }

    n88_renamed = N88BasicAttributeUpdateRules::UpdateAttributes(
        n88_renamed,
        N88BasicFileAttributes{ false, 0x11, true });
    if (!n88_renamed.attributes.is_read_only || n88_renamed.attributes.raw_attributes != 0x11)
    {
        return 43;
    }

    N88BasicDeleteRules::FreeClusters(n88_free_fat, { 1, 2 });
    if (n88_free_fat[1] != 0x00 || n88_free_fat[2] != 0x00)
    {
        return 44;
    }

    const auto n88_formatted_fat = N88BasicFormatRules::CreateFatData(n88_config);
    const auto n88_formatted_dir = N88BasicFormatRules::CreateDirectorySectors(n88_config);
    if (n88_formatted_fat.empty() || n88_formatted_fat[0] != 0x00 ||
        n88_formatted_dir.size() != static_cast<std::size_t>(n88_config.directory_sectors))
    {
        return 45;
    }

    const auto n88_rename_plan = N88BasicShell::PlanRename(n88_directory, n88_config, "TEST.BAS", "DATA.BIN");
    const auto n88_update_plan = N88BasicShell::PlanAttributeUpdate(
        n88_directory,
        n88_config,
        "TEST.BAS",
        N88BasicFileAttributes{ false, 0x11, true });
    const auto n88_delete_plan = N88BasicShell::PlanDelete(n88_fat, n88_chain, n88_directory, n88_config, "TEST.BAS");
    const auto n88_find = N88BasicShell::FindFile(n88_directory, n88_fat, n88_config, "TEST.BAS");
    const auto n88_info = N88BasicShell::GetFileSystemInfo(n88_fat, n88_config);
    const auto n88_write_plan = N88BasicShell::PlanWrite(
        "DATA.BIN",
        { 0x10, 0x20, 0x30 },
        N88BasicFileAttributes{ false, 0x01, false },
        n88_config,
        n88_formatted_fat);
    const auto n88_written_entry = N88BasicFileEntryWriter::Write(
        N88BasicFileEntry{ "DATA", "BIN", 3, N88BasicFileAttributes{ false, 0x01, false }, 1 });
    if (!n88_rename_plan.has_value() ||
        !n88_update_plan.has_value() ||
        !n88_delete_plan.has_value() ||
        !n88_find.has_value() ||
        !n88_write_plan.has_value() ||
        n88_info.cluster_size != n88_config.cluster_size ||
        n88_written_entry[0] != 'D' ||
        n88_delete_plan->entry_offset != 0)
    {
        return 46;
    }

    return 0;
}
