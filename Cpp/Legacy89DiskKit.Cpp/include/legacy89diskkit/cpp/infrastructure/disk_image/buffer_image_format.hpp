#pragma once

#include "legacy89diskkit/cpp/status.hpp"

#include <string>
#include <string_view>

namespace legacy89diskkit::cpp
{
enum class BufferDiskImageFormat : std::uint8_t
{
    D88 = 0,
    Raw = 1,
};

class BufferImageFormat
{
public:
    static Result<BufferDiskImageFormat> Parse(std::string_view image_format);
    static std::string_view ToExtension(BufferDiskImageFormat format);
};
}
