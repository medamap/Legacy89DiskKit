#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <array>
#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
struct D88Header
{
    std::string image_name;
    bool write_protected;
    DiskType media_type;
    std::uint32_t disk_size;
    std::array<std::uint32_t, 164> track_offsets;
};

class D88Parser
{
public:
    static Result<D88Header> ParseHeader(const std::vector<std::uint8_t>& image_data);
    static Result<ReadOnlyDiskImageLayout> ParseImage(const std::vector<std::uint8_t>& image_data);
};
}
