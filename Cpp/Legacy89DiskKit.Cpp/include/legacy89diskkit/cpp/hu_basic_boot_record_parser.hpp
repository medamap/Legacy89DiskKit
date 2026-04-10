#pragma once

#include "legacy89diskkit/cpp/hu_basic_boot_record.hpp"

#include <cstdint>
#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicBootRecordParser
{
public:
    static std::optional<HuBasicBootRecordInfo> Parse(const std::vector<std::uint8_t>& boot_area);
};
}
