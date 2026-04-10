#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class HuBasicModeRules
{
public:
    static HuBasicFileType GetFileType(std::uint8_t mode_byte);
    static std::uint8_t BuildModeByte(const HuBasicFileMetadata& metadata);
    static std::uint8_t BuildModeByte(const HuBasicFileAttributes& attributes);
};
}
