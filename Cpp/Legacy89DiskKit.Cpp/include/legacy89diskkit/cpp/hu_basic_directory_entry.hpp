#pragma once

#include <array>
#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
{
struct HuBasicDirectoryEntry
{
    std::uint8_t mode_byte;
    std::uint8_t password_byte;
    std::array<std::uint8_t, 13> raw_file_name;
    std::array<std::uint8_t, 3> raw_extension;
    std::string file_name;
    std::string extension;
    std::uint16_t recorded_size;
    std::uint16_t load_address;
    std::uint16_t execution_address;
    int start_cluster;
};
}
