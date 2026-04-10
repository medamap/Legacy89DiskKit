#pragma once

#include "legacy89diskkit/cpp/domain/controller/controller_runtime_contracts.hpp"
#include "legacy89diskkit/cpp/domain/drive/mounted_medium_contracts.hpp"
#include "legacy89diskkit/cpp/domain/drive/sector_addressable_medium_contracts.hpp"

#include <memory>

namespace legacy89diskkit::cpp
{
struct MountedMediumBinding
{
    std::shared_ptr<MountedMedium> mounted_medium;
    std::shared_ptr<SectorAddressableMedium> sector_medium;
    std::shared_ptr<ControllerFacingMedium> controller_facing_medium;
};
}
