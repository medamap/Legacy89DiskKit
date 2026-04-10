#pragma once

#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
{
struct HuBasicBootRecordInfo
{
    std::uint8_t boot_flag;
    std::string file_name;
    std::string extension;
    bool has_password;
    std::uint16_t size;
    std::uint16_t load_address;
    std::uint16_t execution_address;
    std::uint16_t start_record;
};
}
