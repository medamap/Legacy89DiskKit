#pragma once

#include "legacy89diskkit/cpp/domain/raw/raw_preservation_types.hpp"

#include <optional>
#include <vector>

namespace legacy89diskkit::cpp
{
enum class RawConversionDirection : std::uint8_t
{
    SectorImageToEncodedTrack = 0,
    EncodedTrackToSectorImage = 1,
    LowerLevelSignalToEncodedTrack = 2,
};

enum class RawConversionLossiness : std::uint8_t
{
    Lossless = 0,
    PotentiallyLossy = 1,
    Unsupported = 2,
};

struct RawConversionContract
{
    RawConversionDirection direction;
    RawConversionLossiness lossiness;
    bool preserves_sector_payloads;
    bool requires_geometry_hint;
    bool requires_capture_metadata;
};

class RawConversionContractCatalog
{
public:
    static std::vector<RawConversionContract> GetContracts()
    {
        return {
            {RawConversionDirection::SectorImageToEncodedTrack, RawConversionLossiness::PotentiallyLossy, true, true, false},
            {RawConversionDirection::EncodedTrackToSectorImage, RawConversionLossiness::PotentiallyLossy, false, false, false},
            {RawConversionDirection::LowerLevelSignalToEncodedTrack, RawConversionLossiness::Lossless, false, false, true},
        };
    }

    static std::optional<RawConversionContract> Find(RawConversionDirection direction)
    {
        for (const auto contract : GetContracts())
        {
            if (contract.direction == direction)
            {
                return contract;
            }
        }

        return std::nullopt;
    }
};
}
