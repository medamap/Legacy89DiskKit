#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/filesystem_detection.hpp"

#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateHuBasicImage()
{
    std::vector<std::uint8_t> image(327680, 0x00);
    image[0] = 0x01;
    image[0x0e] = 'S';
    image[0x0f] = 'y';
    image[0x10] = 's';
    return image;
}

std::vector<std::uint8_t> CreateN88BasicImage()
{
    std::vector<std::uint8_t> image(327680, 0x00);
    const auto sector_size = 256;
    const auto sectors_per_track = 16;
    const auto offset = (((9 * 2) + 1) * sectors_per_track + (13 - 1)) * sector_size;
    image[offset] = 0xfe;
    const auto dir_offset = (((9 * 2) + 1) * sectors_per_track + (1 - 1)) * sector_size;
    image[dir_offset] = 0xff;
    return image;
}

std::vector<std::uint8_t> CreateMsxDosImage()
{
    std::vector<std::uint8_t> image(737280, 0x00);
    image[0] = 0xeb;
    return image;
}
}

int main()
{
    auto hu_container = RawDiskContainer::OpenFromBuffer(CreateHuBasicImage(), true);
    if (!hu_container.ok())
    {
        return 1;
    }

    const auto hu_best = FileSystemDetection::DetectBest(hu_container.value());
    if (hu_best == nullptr || hu_best->family != FileSystemFamily::HuBasic)
    {
        return 2;
    }

    auto n88_container = RawDiskContainer::OpenFromBuffer(CreateN88BasicImage(), true);
    if (!n88_container.ok())
    {
        return 3;
    }

    const auto n88_best = FileSystemDetection::DetectBest(n88_container.value());
    if (n88_best == nullptr || n88_best->family != FileSystemFamily::N88Basic)
    {
        return 4;
    }

    auto msx_container = RawDiskContainer::OpenFromBuffer(CreateMsxDosImage(), true);
    if (!msx_container.ok())
    {
        return 5;
    }

    const auto msx_best = FileSystemDetection::DetectBest(msx_container.value());
    if (msx_best == nullptr || msx_best->family != FileSystemFamily::MsxDos)
    {
        return 6;
    }

    const auto hu_candidates = FileSystemDetection::DetectCandidates(hu_container.value());
    if (hu_candidates.empty() || hu_candidates.front().canonical_name != "Hu-BASIC")
    {
        return 7;
    }

    return 0;
}
