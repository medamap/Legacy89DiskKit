#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/explicit_filesystem_selector.hpp"

#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateRawTwoDImage()
{
    return std::vector<std::uint8_t>(327680, 0x00);
}

std::vector<std::uint8_t> CreateRawTwoDDImage()
{
    return std::vector<std::uint8_t>(737280, 0x00);
}
}

int main()
{
    auto raw_2d = RawDiskContainer::OpenFromBuffer(CreateRawTwoDImage(), false);
    if (!raw_2d.ok())
    {
        return 1;
    }

    auto hu = ExplicitFileSystemSelector::Open("Hu-BASIC", raw_2d.value());
    if (!hu.ok() || ExplicitFileSystemSelector::GetFamily(hu.value()) != FileSystemFamily::HuBasic)
    {
        return 2;
    }

    auto n88 = ExplicitFileSystemSelector::Open("n88basic", raw_2d.value());
    if (!n88.ok() || ExplicitFileSystemSelector::GetFamily(n88.value()) != FileSystemFamily::N88Basic)
    {
        return 3;
    }

    auto invalid = ExplicitFileSystemSelector::Open("unknown", raw_2d.value());
    if (invalid.ok())
    {
        return 4;
    }

    auto msx_2d = ExplicitFileSystemSelector::Open("MSX-DOS", raw_2d.value());
    if (msx_2d.ok())
    {
        return 5;
    }

    auto raw_2dd = RawDiskContainer::OpenFromBuffer(CreateRawTwoDDImage(), false);
    if (!raw_2dd.ok())
    {
        return 6;
    }

    auto msx = ExplicitFileSystemSelector::Open("msxdos", raw_2dd.value());
    if (!msx.ok() || ExplicitFileSystemSelector::GetFamily(msx.value()) != FileSystemFamily::MsxDos)
    {
        return 7;
    }

    const auto parsed = ExplicitFileSystemSelector::ParseFamily("Hu Basic");
    if (!parsed.ok() || parsed.value() != FileSystemFamily::HuBasic)
    {
        return 8;
    }

    if (ExplicitFileSystemSelector::GetCanonicalName(FileSystemFamily::MsxDos) != "MSX-DOS")
    {
        return 9;
    }

    return 0;
}
