#include "legacy89diskkit/cpp/infrastructure/drive/d88_sector_addressable_medium.hpp"

namespace legacy89diskkit::cpp
{
namespace
{
const std::string kD88MediumKind = "d88-family";
}

D88SectorAddressableMedium::D88SectorAddressableMedium(D88DiskContainer& container)
    : container_(&container)
{
}

const std::string& D88SectorAddressableMedium::MediumKind() const
{
    return kD88MediumKind;
}

bool D88SectorAddressableMedium::SupportsDirectImageAccess() const
{
    return true;
}

bool D88SectorAddressableMedium::SupportsControllerFacingAccess() const
{
    return true;
}

bool D88SectorAddressableMedium::SectorExists(const int cylinder, const int head, const int sector) const
{
    return container_->SectorExists(cylinder, head, sector);
}

std::vector<std::uint8_t> D88SectorAddressableMedium::ReadSector(
    const int cylinder,
    const int head,
    const int sector,
    const bool) const
{
    const auto result = container_->ReadSector(cylinder, head, sector);
    return result.ok() ? result.value() : std::vector<std::uint8_t>{};
}

std::vector<SectorInfo> D88SectorAddressableMedium::GetAllSectors() const
{
    return container_->GetAllSectors();
}
}
