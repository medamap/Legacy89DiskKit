#include "legacy89diskkit/cpp/disk_image_types.hpp"
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
    const auto two_hd = HuBasicReadRules::ResolveReadPayload(
        { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
        two_hd_file,
        DiskType::TwoHD,
        HuBasicConfiguration{ 0x10, 0x100, 1024, 256 },
        1,
        0x82);
    if (two_hd.size() != 512)
    {
        if (two_hd.size() != 10)
        {
            return 6;
        }
    }

    return 0;
}
