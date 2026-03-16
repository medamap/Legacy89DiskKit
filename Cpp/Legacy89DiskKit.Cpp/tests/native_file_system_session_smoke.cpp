#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include <filesystem>
#include <fstream>
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
}

int main()
{
    const auto image_path = std::filesystem::temp_directory_path() / "ldk-native-session-smoke.img";
    {
        std::ofstream stream(image_path, std::ios::binary | std::ios::trunc);
        const auto image = CreateHuBasicImage();
        stream.write(reinterpret_cast<const char*>(image.data()), static_cast<std::streamsize>(image.size()));
    }

    const auto detected = NativeFileSystemSession::Open(image_path, false);
    if (!detected.ok())
    {
        std::filesystem::remove(image_path);
        return 1;
    }

    if (detected.value().Family() != FileSystemFamily::HuBasic)
    {
        std::filesystem::remove(image_path);
        return 2;
    }

    const auto info = detected.value().GetFileSystemInfo();
    if (info.file_system_name != "Hu-BASIC" || info.platform_id != "X1")
    {
        std::filesystem::remove(image_path);
        return 3;
    }

    const auto explicit_open = NativeFileSystemSession::Open(image_path, true, FileSystemFamily::HuBasic);
    std::filesystem::remove(image_path);
    if (!explicit_open.ok() || !explicit_open.value().IsReadOnly())
    {
        return 4;
    }

    return 0;
}
