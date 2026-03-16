#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_buffer_loader.hpp"

#include "legacy89diskkit/cpp/d88_parser.hpp"

namespace legacy89diskkit::cpp
{
Result<ReadOnlyDiskImageLayout> D88BufferLoader::Load(const std::span<const std::uint8_t> image_data)
{
    if (image_data.empty())
    {
        return Result<ReadOnlyDiskImageLayout>::Failure(StatusCode::InvalidArgument, "Image buffer must not be empty.");
    }

    return D88Parser::ParseImage(std::vector<std::uint8_t>(image_data.begin(), image_data.end()));
}
}
