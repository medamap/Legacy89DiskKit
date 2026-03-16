#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/n88_basic_types.hpp"

namespace legacy89diskkit::cpp
{
class N88BasicConfigurationProvider
{
public:
    static N88BasicConfiguration GetDefault(DiskType disk_type);
};
}
