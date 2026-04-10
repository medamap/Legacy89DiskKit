#include "legacy89diskkit/cpp/domain/raw/encoded_track_contracts.hpp"
#include "legacy89diskkit/cpp/domain/raw/raw_conversion_contracts.hpp"

#include <span>
#include <string>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
class StubEncodedTrackSurface final : public EncodedTrackDomainSurface
{
public:
    StubEncodedTrackSurface()
        : tracks_{
              {
                  {0, 0},
                  RawTrackEncoding::Mfm,
                  {0xa1, 0xa1, 0xa1, 0xfe, 0x00, 0x00, 0x01},
                  std::vector<std::uint8_t>{0xff, 0x00, 0xff, 0x00},
                  true,
              },
          }
    {
    }

    RawPreservationIdentity GetIdentity() const override
    {
        return {"Legacy 89 Storage", ".l89", "provisional"};
    }

    RawPreservationMetadataSummary GetMetadataSummary() const override
    {
        return {RawPreservationTier::EncodedTrack, tracks_.size(), true, true};
    }

    RawPreservationIntegrityInfo GetIntegrityInfo() const override
    {
        return {RawIntegrityKind::TrackChecksum, true, false};
    }

    std::span<const EncodedTrackPayload> GetTracks() const override
    {
        return tracks_;
    }

private:
    std::vector<EncodedTrackPayload> tracks_;
};
}

int main()
{
    StubEncodedTrackSurface surface;
    const auto identity = surface.GetIdentity();
    const auto metadata = surface.GetMetadataSummary();
    const auto integrity = surface.GetIntegrityInfo();
    const auto tracks = surface.GetTracks();
    const auto export_contract = RawConversionContractCatalog::Find(RawConversionDirection::SectorImageToEncodedTrack);
    const auto import_contract = RawConversionContractCatalog::Find(RawConversionDirection::EncodedTrackToSectorImage);

    if (identity.family_name != "Legacy 89 Storage")
    {
        return 1;
    }

    if (identity.extension != ".l89")
    {
        return 2;
    }

    if (metadata.tier != RawPreservationTier::EncodedTrack || metadata.track_count != 1)
    {
        return 3;
    }

    if (integrity.kind != RawIntegrityKind::TrackChecksum || !integrity.per_track || integrity.whole_container)
    {
        return 4;
    }

    if (tracks.size() != 1 || tracks[0].encoding != RawTrackEncoding::Mfm || tracks[0].PayloadBytes() != 7)
    {
        return 5;
    }

    if (!tracks[0].clock_map.has_value() || !tracks[0].has_index_alignment)
    {
        return 6;
    }

    if (!export_contract.has_value() || export_contract->lossiness != RawConversionLossiness::PotentiallyLossy || !export_contract->preserves_sector_payloads)
    {
        return 7;
    }

    if (!import_contract.has_value() || import_contract->lossiness != RawConversionLossiness::PotentiallyLossy || import_contract->preserves_sector_payloads)
    {
        return 8;
    }

    return 0;
}
