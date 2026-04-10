#pragma once

#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/drive/mounted_medium_binding.hpp"

namespace legacy89diskkit::cpp
{
class MountedMediumBindingFactory
{
public:
    static MountedMediumBinding Create(RawDiskContainer& container);
    static MountedMediumBinding Create(D88DiskContainer& container);
};
}
