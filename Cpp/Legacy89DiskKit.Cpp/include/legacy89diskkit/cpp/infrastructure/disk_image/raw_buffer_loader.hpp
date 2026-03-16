#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <span>

namespace legacy89diskkit::cpp
{
class RawBufferLoader
{
public:
    static Result<ReadOnlyDiskImageLayout> Load(std::span<const std::uint8_t> image_data);
};
}
