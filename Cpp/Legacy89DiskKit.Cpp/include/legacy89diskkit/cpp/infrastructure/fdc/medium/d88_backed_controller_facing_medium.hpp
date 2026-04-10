#pragma once

#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/fdc/medium/sector_backed_controller_facing_medium.hpp"

namespace legacy89diskkit::cpp
{
class D88BackedControllerFacingMedium final : public SectorBackedControllerFacingMedium
{
public:
    explicit D88BackedControllerFacingMedium(D88DiskContainer& container);

    const std::string& MediumKind() const override;
    bool IsWriteProtected() const override;

protected:
    bool SectorExistsCore(int track, int side, int sector) const override;
    std::vector<std::uint8_t> ReadSectorCore(int track, int side, int sector) const override;

private:
    D88DiskContainer* container_;
};
}
