#include "legacy89diskkit_native.h"

#include <filesystem>
#include <fstream>
#include <string>
#include <vector>

namespace
{
std::filesystem::path WriteHuBasicImage()
{
    const auto image_path = std::filesystem::temp_directory_path() / "ldk-cpp-native-bridge-smoke.img";

    std::vector<std::uint8_t> image(40 * 2 * 16 * 256, 0x00);
    image[0x1000] = 'H';
    image[0x1001] = 'E';
    image[0x1002] = 'L';
    image[0x1003] = 'L';
    image[0x1004] = 'O';

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

    char operation[64]{};
    if (ldk_get_handle_source_operation(handle, operation, 64) <= 0 || std::string(operation) != "open-disk")
    {
        ldk_close_all_handles();
        std::filesystem::remove(image_path);
        return 1;
    }

    if (ldk_get_handle_is_writable(handle) != 0)
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

    ldk_close_all_handles();
    std::filesystem::remove(image_path);
    return 0;
}
