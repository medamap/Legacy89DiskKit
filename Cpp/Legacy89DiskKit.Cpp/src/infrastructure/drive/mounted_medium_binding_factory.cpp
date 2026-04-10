#include "legacy89diskkit/cpp/infrastructure/drive/mounted_medium_binding_factory.hpp"

#include "legacy89diskkit/cpp/infrastructure/drive/d88_sector_addressable_medium.hpp"
#include "legacy89diskkit/cpp/infrastructure/drive/raw_sector_addressable_medium.hpp"
#include "legacy89diskkit/cpp/infrastructure/fdc/medium/d88_backed_controller_facing_medium.hpp"
#include "legacy89diskkit/cpp/infrastructure/fdc/medium/raw_backed_controller_facing_medium.hpp"

namespace legacy89diskkit::cpp
{
MountedMediumBinding MountedMediumBindingFactory::Create(RawDiskContainer& container)
{
    auto sector_medium = std::make_shared<RawSectorAddressableMedium>(container);
    auto controller_medium = std::make_shared<RawBackedControllerFacingMedium>(container);
    return {sector_medium, sector_medium, controller_medium};
}

MountedMediumBinding MountedMediumBindingFactory::Create(D88DiskContainer& container)
{
    auto sector_medium = std::make_shared<D88SectorAddressableMedium>(container);
    auto controller_medium = std::make_shared<D88BackedControllerFacingMedium>(container);
    return {sector_medium, sector_medium, controller_medium};
}
}
