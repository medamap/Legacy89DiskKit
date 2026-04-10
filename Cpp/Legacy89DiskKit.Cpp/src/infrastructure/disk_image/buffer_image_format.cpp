#include "legacy89diskkit/cpp/infrastructure/disk_image/buffer_image_format.hpp"

#include <cctype>

namespace legacy89diskkit::cpp
{
Result<BufferDiskImageFormat> BufferImageFormat::Parse(const std::string_view image_format)
{
    if (image_format.empty())
    {
        return Result<BufferDiskImageFormat>::Failure(StatusCode::InvalidArgument, "Image format must be specified.");
    }

    std::string normalized;
    normalized.reserve(image_format.size() + 1);
    if (!image_format.starts_with('.'))
    {
        normalized.push_back('.');
    }

    for (const auto ch : image_format)
    {
        normalized.push_back(static_cast<char>(std::tolower(static_cast<unsigned char>(ch))));
    }

    if (normalized == ".d88" || normalized == ".d77")
    {
        return Result<BufferDiskImageFormat>::Success(BufferDiskImageFormat::D88);
    }

    if (normalized == ".2d" || normalized == ".dsk")
    {
        return Result<BufferDiskImageFormat>::Success(BufferDiskImageFormat::Raw);
    }

    return Result<BufferDiskImageFormat>::Failure(StatusCode::UnsupportedFormat, "Unsupported disk image format.");
}

std::string_view BufferImageFormat::ToExtension(const BufferDiskImageFormat format)
{
    switch (format)
    {
    case BufferDiskImageFormat::D88:
        return ".d88";
    case BufferDiskImageFormat::Raw:
        return ".2d";
    }

    return {};
}
}
