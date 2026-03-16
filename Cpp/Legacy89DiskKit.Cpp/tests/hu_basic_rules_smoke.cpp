#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/hu_basic_directory_entry_codec.hpp"
#include "legacy89diskkit/cpp/hu_basic_fat_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_name_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_read_rules.hpp"
#include "legacy89diskkit/cpp/hu_basic_types.hpp"

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

    HuBasicConfiguration config{ 0x10, 0x100, 1024, 256 };
    std::vector<std::uint8_t> fat(256, 0x00);
    HuBasicFatRules::ApplyChain(fat, { 0x10, 0x11 }, 0x83);
    const auto chain = HuBasicFatRules::GetClusterChain(fat, config, 0x10);
    if (chain.chain.size() != 2 || chain.terminal_flag != 0x83)
    {
        return 3;
    }

    HuBasicFileEntry binary_file{ 3, HuBasicFileAttributes{ false } };
    const auto trimmed = HuBasicReadRules::ResolveReadPayload({ 0x10, 0x20, 0x30, 0x40 }, binary_file, DiskType::TwoD, config, 1, 0xff);
    if (trimmed.size() != 3 || trimmed[2] != 0x30)
    {
        return 4;
    }

    HuBasicFileEntry ascii_file{ 0, HuBasicFileAttributes{ true } };
    const auto ascii = HuBasicReadRules::ResolveReadPayload({ 'A', 'B', 0x1a, 'C' }, ascii_file, DiskType::TwoD, config, 1, 0xff);
    if (ascii.size() != 2 || ascii[0] != 'A' || ascii[1] != 'B')
    {
        return 5;
    }

    HuBasicFileEntry two_hd_file{ 0, HuBasicFileAttributes{ false } };
    std::vector<std::uint8_t> two_hd_data(700, 0x5a);
    const auto two_hd = HuBasicReadRules::ResolveReadPayload(
        two_hd_data,
        two_hd_file,
        DiskType::TwoHD,
        HuBasicConfiguration{ 0x10, 0x100, 1024, 256 },
        1,
        0x82);
    if (two_hd.size() != 768)
    {
        return 6;
    }

    std::array<std::uint8_t, 32> entry_bytes{};
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
        return 7;
    }

    const auto roundtrip = HuBasicDirectoryEntryCodec::Write(entry);
    if (roundtrip[1] != 'H' || roundtrip[0x0e] != 'B' || roundtrip[0x12] != 0x34 || roundtrip[0x13] != 0x12)
    {
        return 8;
    }

    return 0;
}
