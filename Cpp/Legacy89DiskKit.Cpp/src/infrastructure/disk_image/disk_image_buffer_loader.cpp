#include "legacy89diskkit/cpp/infrastructure/disk_image/disk_image_buffer_loader.hpp"

#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_buffer_loader.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_buffer_loader.hpp"

namespace legacy89diskkit::cpp
{
Result<ReadOnlyDiskImageLayout> DiskImageBufferLoader::Load(
    const std::span<const std::uint8_t> image_data,
    const BufferDiskImageFormat format)
{
    switch (format)
    {
    case BufferDiskImageFormat::D88:
        return D88BufferLoader::Load(image_data);
    case BufferDiskImageFormat::Raw:
        return RawBufferLoader::Load(image_data);
    }

    return Result<ReadOnlyDiskImageLayout>::Failure(StatusCode::UnsupportedFormat, "Unsupported disk image format.");
}

Result<ReadOnlyDiskImageLayout> DiskImageBufferLoader::Load(
    const std::span<const std::uint8_t> image_data,
    const std::string_view image_format)
{
    const auto format_result = BufferImageFormat::Parse(image_format);
    if (!format_result.ok())
    {
        return Result<ReadOnlyDiskImageLayout>::Failure(format_result.status().code, format_result.status().message);
    }

    return Load(image_data, format_result.value());
}
}
