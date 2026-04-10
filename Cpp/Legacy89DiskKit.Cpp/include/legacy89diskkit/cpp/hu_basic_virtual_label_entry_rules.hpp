#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
{
class HuBasicVirtualLabelEntryRules
{
public:
    static HuBasicFileEntry CreateEntry(
        const std::string& file_name,
        const std::string& extension,
        std::uint8_t raw_mode_byte,
        std::uint8_t password_byte,
        std::uint16_t size,
        std::uint16_t load_address,
        std::uint16_t end_address,
        std::uint16_t execution_address,
        int start_cluster);
};
}
