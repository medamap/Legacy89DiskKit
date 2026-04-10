#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include <filesystem>
#include <fstream>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
struct TempFile
{
    std::filesystem::path path;
    explicit TempFile(std::filesystem::path p) : path(std::move(p)) 
    {
        if (std::filesystem::exists(path)) std::filesystem::remove(path);
    }
    ~TempFile()
    {
        if (std::filesystem::exists(path)) std::filesystem::remove(path);
    }
    std::string string() const { return path.string(); }
    operator std::filesystem::path() const { return path; }
};

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
    TempFile image_file(std::filesystem::temp_directory_path() / "ldk-native-session-smoke.img");
    const auto image_path = image_file.path;
    
    {
        std::ofstream stream(image_path, std::ios::binary | std::ios::trunc);
        const auto image = CreateHuBasicImage();
        stream.write(reinterpret_cast<const char*>(image.data()), static_cast<std::streamsize>(image.size()));
    }

    {
        // 1. Path-based open with .img (Raw routing)
        const auto opened = NativeFileSystemSession::Open(image_path, true);
        if (!opened.ok() || opened.value().Family() != FileSystemFamily::HuBasic)
        {
            return 11;
        }

        // Verify it use Raw backend for .img
        const auto metadata = opened.value().GetContainerMetadata();
        if (metadata.image_format != "raw-sector-container")
        {
            return 12;
        }
    }

    {
        // 2. Buffer-based open with explicit Raw hint
        const auto image = CreateHuBasicImage();
        const auto opened = NativeFileSystemSession::OpenFromBuffer(image, true, BufferDiskImageFormat::Raw);
        if (!opened.ok() || opened.value().Family() != FileSystemFamily::HuBasic)
        {
            return 21;
        }

        const auto metadata = opened.value().GetContainerMetadata();
        if (metadata.image_format != "raw-sector-container")
        {
            return 22;
        }
    }

    {
        // 3. Buffer-based open with Auto hint (Probing)
        const auto image = CreateHuBasicImage();
        const auto opened = NativeFileSystemSession::OpenFromBuffer(image, true, std::nullopt);
        if (!opened.ok() || opened.value().Family() != FileSystemFamily::HuBasic)
        {
            return 31;
        }
    }

    // 4. Auto detection from path (verification of detected properties)
    const auto detected = NativeFileSystemSession::Open(image_path, false);
    if (!detected.ok())
    {
        return 41;
    }

    if (detected.value().Family() != FileSystemFamily::HuBasic)
    {
        return 42;
    }

    const auto info = detected.value().GetFileSystemInfo();
    if (info.file_system_name != "Hu-BASIC" || info.platform_id != "X1")
    {
        return 43;
    }

    // 5. Explicit family open
    const auto explicit_open = NativeFileSystemSession::Open(image_path, true, FileSystemFamily::HuBasic);
    if (!explicit_open.ok() || !explicit_open.value().IsReadOnly())
    {
        return 51;
    }

    return 0;
}

