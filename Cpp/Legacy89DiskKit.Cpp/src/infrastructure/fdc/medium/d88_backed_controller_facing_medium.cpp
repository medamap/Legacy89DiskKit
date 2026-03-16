#include "legacy89diskkit/cpp/infrastructure/fdc/medium/d88_backed_controller_facing_medium.hpp"

namespace legacy89diskkit::cpp
{
namespace
{
const std::string kD88MediumKind = "d88-family";
}

D88BackedControllerFacingMedium::D88BackedControllerFacingMedium(D88DiskContainer& container)
    : container_(&container)
{
}

const std::string& D88BackedControllerFacingMedium::MediumKind() const
{
    return kD88MediumKind;
}

bool D88BackedControllerFacingMedium::IsWriteProtected() const
{
    return container_->IsReadOnly();
}

bool D88BackedControllerFacingMedium::SectorExistsCore(const int track, const int side, const int sector) const
{
    return container_->SectorExists(track, side, sector);
}

std::vector<std::uint8_t> D88BackedControllerFacingMedium::ReadSectorCore(
    const int track,
    const int side,
    const int sector) const
{
    const auto result = container_->ReadSector(track, side, sector);
    return result.ok() ? result.value() : std::vector<std::uint8_t>{};
}
}
