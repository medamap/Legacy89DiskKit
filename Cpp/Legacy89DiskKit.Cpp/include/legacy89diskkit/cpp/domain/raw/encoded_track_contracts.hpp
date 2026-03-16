#pragma once

#include "legacy89diskkit/cpp/domain/raw/raw_preservation_types.hpp"

#include <optional>
#include <span>
#include <vector>

namespace legacy89diskkit::cpp
{
struct EncodedTrackPayload
{
    RawTrackAddress address;
    RawTrackEncoding encoding;
    std::vector<std::uint8_t> payload;
    std::optional<std::vector<std::uint8_t>> clock_map;
    bool has_index_alignment;

    std::size_t PayloadBytes() const
    {
        return payload.size();
    }
};

class EncodedTrackDomainSurface
{
public:
    virtual ~EncodedTrackDomainSurface() = default;

    virtual RawPreservationIdentity GetIdentity() const = 0;
    virtual RawPreservationMetadataSummary GetMetadataSummary() const = 0;
    virtual RawPreservationIntegrityInfo GetIntegrityInfo() const = 0;
    virtual std::span<const EncodedTrackPayload> GetTracks() const = 0;
};
}
