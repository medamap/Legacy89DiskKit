#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/buffer_image_format.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <span>
#include <vector>

namespace legacy89diskkit::cpp
{
class DiskImageBufferLoader
{
public:
    static Result<ReadOnlyDiskImageLayout> Load(
        std::span<const std::uint8_t> image_data,
        BufferDiskImageFormat format);

    static Result<ReadOnlyDiskImageLayout> Load(
        std::span<const std::uint8_t> image_data,
        std::string_view image_format);
};
}
