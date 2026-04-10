#pragma once

#include "legacy89diskkit/cpp/disk_image_types.hpp"
#include "legacy89diskkit/cpp/domain/drive/mounted_medium_contracts.hpp"

#include <cstdint>
#include <vector>

namespace legacy89diskkit::cpp
{
class SectorAddressableMedium : public MountedMedium
{
public:
    virtual ~SectorAddressableMedium() = default;

    virtual bool SectorExists(int cylinder, int head, int sector) const = 0;
    virtual std::vector<std::uint8_t> ReadSector(
        int cylinder,
        int head,
        int sector,
        bool allow_corrupted = false) const = 0;
    virtual std::vector<SectorInfo> GetAllSectors() const = 0;
};
}
