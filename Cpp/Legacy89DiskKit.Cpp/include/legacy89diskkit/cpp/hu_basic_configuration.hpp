#pragma once

#include "legacy89diskkit/cpp/hu_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class HuBasicConfigurationProvider
{
public:
    static HuBasicConfiguration GetDefault(DiskType disk_type);
};
}
