#pragma once

#include "legacy89diskkit/cpp/hu_basic_boot_record.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class HuBasicBootRecordCodec
{
public:
    static std::vector<std::uint8_t> Write(const HuBasicBootRecordInfo& record);
};
}
