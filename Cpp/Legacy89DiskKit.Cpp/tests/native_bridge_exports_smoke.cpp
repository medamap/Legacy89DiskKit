#include "legacy89diskkit_native.h"

#include <filesystem>
#include <fstream>
#include <string>
#include <vector>

namespace
{
std::vector<std::uint8_t> CreateHuBasicImageBuffer()
{
    std::vector<std::uint8_t> image(327680, 0x00);
    image[0] = 0x01;
    image[0x0e] = 'S';
    image[0x0f] = 'y';
    image[0x10] = 's';
    return image;
}

std::filesystem::path WriteHuBasicImage()
{
    const auto image_path = std::filesystem::temp_directory_path() / "ldk-cpp-native-bridge-smoke.img";
    const auto image = CreateHuBasicImageBuffer();

    std::ofstream stream(image_path, std::ios::binary | std::ios::trunc);
    stream.write(reinterpret_cast<const char*>(image.data()), static_cast<std::streamsize>(image.size()));
    return image_path;
}
}

int main()
{
    const auto image_path = WriteHuBasicImage();
    const auto handle = ldk_open_disk(image_path.string().c_str(), 1);
    if (handle <= 0)
    {
        std::filesystem::remove(image_path);
        return 1;
    }

    char backend_kind[64]{};
    if (ldk_get_backend_kind(backend_kind, 64) <= 0 || std::string(backend_kind) != "cpp-bridge")
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 1;
    }

    if (ldk_is_handle_valid(handle) != 1 || ldk_get_open_handle_count() < 1)
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 1;
    }

    if (ldk_close_disk(handle) != LDK_STATUS_SUCCESS || ldk_is_handle_valid(handle) != 0)
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 1;
    }

    // Test OpenFromBuffer
    const auto image_data = CreateHuBasicImageBuffer();

    const auto buffer_handle = ldk_open_disk_from_buffer(image_data.data(), static_cast<std::int32_t>(image_data.size()), 1);
    if (buffer_handle <= 0)
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 1;
    }

    // Test Handle Metadata
    char source[64]{};
    if (ldk_get_handle_source_operation(buffer_handle, source, 64) <= 0 || 
        std::string(source) != "open-disk-from-buffer")
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 103;
    }

    if (ldk_get_handle_is_writable(buffer_handle) != 0) // read_only=1 was passed
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 104;
    }

    if (ldk_close_disk(buffer_handle) != LDK_STATUS_SUCCESS)
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 1;
    }

    // Test Writable Handle
    const auto write_handle = ldk_open_disk_from_buffer(image_data.data(), static_cast<std::int32_t>(image_data.size()), 0);
    if (ldk_get_handle_is_writable(write_handle) != 1)
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 105;
    }
    ldk_close_disk(write_handle);

    ldk_close_all_handles();
    std::filesystem::remove(image_path);
    return 0;
}
