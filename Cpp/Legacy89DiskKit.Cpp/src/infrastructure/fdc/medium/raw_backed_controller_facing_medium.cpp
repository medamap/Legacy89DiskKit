#include "legacy89diskkit/cpp/infrastructure/fdc/medium/raw_backed_controller_facing_medium.hpp"

namespace legacy89diskkit::cpp
{
namespace
{
const std::string kRawMediumKind = "raw-sector-image";
}

RawBackedControllerFacingMedium::RawBackedControllerFacingMedium(RawDiskContainer& container)
    : container_(&container)
{
}

const std::string& RawBackedControllerFacingMedium::MediumKind() const
{
    return kRawMediumKind;
}

bool RawBackedControllerFacingMedium::IsWriteProtected() const
{
    return container_->IsReadOnly();
}

bool RawBackedControllerFacingMedium::SectorExistsCore(const int track, const int side, const int sector) const
{
    return container_->SectorExists(track, side, sector);
}

std::vector<std::uint8_t> RawBackedControllerFacingMedium::ReadSectorCore(
    const int track,
    const int side,
    const int sector) const
{
    const auto result = container_->ReadSector(track, side, sector);
    return result.ok() ? result.value() : std::vector<std::uint8_t>{};
}
}
