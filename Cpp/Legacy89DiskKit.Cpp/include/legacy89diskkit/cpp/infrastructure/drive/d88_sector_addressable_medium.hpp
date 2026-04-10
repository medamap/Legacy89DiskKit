#pragma once

#include "legacy89diskkit/cpp/domain/drive/sector_addressable_medium_contracts.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"

namespace legacy89diskkit::cpp
{
class D88SectorAddressableMedium final : public SectorAddressableMedium
{
public:
    explicit D88SectorAddressableMedium(D88DiskContainer& container);

    const std::string& MediumKind() const override;
    bool SupportsDirectImageAccess() const override;
    bool SupportsControllerFacingAccess() const override;
    bool SectorExists(int cylinder, int head, int sector) const override;
    std::vector<std::uint8_t> ReadSector(int cylinder, int head, int sector, bool allow_corrupted = false) const override;
    std::vector<SectorInfo> GetAllSectors() const override;

private:
    D88DiskContainer* container_;
};
}
