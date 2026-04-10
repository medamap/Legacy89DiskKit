#pragma once

#include <array>
#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
{
struct MsxDosDirectoryEntry
{
    std::array<std::uint8_t, 8> raw_file_name;
    std::array<std::uint8_t, 3> raw_extension;
    std::string file_name;
    std::string extension;
    std::uint8_t attribute_byte;
    std::uint16_t write_time;
    std::uint16_t write_date;
    int start_cluster;
    std::uint32_t size;
};
}
