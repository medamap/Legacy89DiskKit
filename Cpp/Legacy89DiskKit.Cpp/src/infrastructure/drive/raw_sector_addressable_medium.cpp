#include "legacy89diskkit/cpp/infrastructure/drive/raw_sector_addressable_medium.hpp"

namespace legacy89diskkit::cpp
{
namespace
{
const std::string kRawMediumKind = "raw-sector-image";
}

RawSectorAddressableMedium::RawSectorAddressableMedium(RawDiskContainer& container)
    : container_(&container)
{
}

const std::string& RawSectorAddressableMedium::MediumKind() const
{
    return kRawMediumKind;
}

bool RawSectorAddressableMedium::SupportsDirectImageAccess() const
{
    return true;
}

bool RawSectorAddressableMedium::SupportsControllerFacingAccess() const
{
    return true;
}

bool RawSectorAddressableMedium::SectorExists(const int cylinder, const int head, const int sector) const
{
    return container_->SectorExists(cylinder, head, sector);
}

std::vector<std::uint8_t> RawSectorAddressableMedium::ReadSector(
    const int cylinder,
    const int head,
    const int sector,
    const bool) const
{
    const auto result = container_->ReadSector(cylinder, head, sector);
    return result.ok() ? result.value() : std::vector<std::uint8_t>{};
}

std::vector<SectorInfo> RawSectorAddressableMedium::GetAllSectors() const
{
    return container_->GetAllSectors();
}
}
