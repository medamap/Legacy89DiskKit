#pragma once

#include <array>
#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
{
struct N88BasicDirectoryEntry
{
    std::array<std::uint8_t, 6> raw_file_name;
    std::array<std::uint8_t, 3> raw_extension;
    std::string file_name;
    std::string extension;
    std::uint8_t attribute_byte;
    int start_cluster;
};
}
