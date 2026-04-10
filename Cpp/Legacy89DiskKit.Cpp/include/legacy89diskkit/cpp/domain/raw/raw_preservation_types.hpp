#pragma once

#include <cstddef>
#include <cstdint>
#include <string_view>

namespace legacy89diskkit::cpp
{
enum class RawPreservationTier : std::uint8_t
{
    EncodedTrack = 0,
    LowerLevelSignal = 1,
};

enum class RawTrackEncoding : std::uint8_t
{
    Unknown = 0,
    Fm = 1,
    Mfm = 2,
    Mixed = 3,
};

enum class RawIntegrityKind : std::uint8_t
{
    None = 0,
    TrackChecksum = 1,
    ContainerDigest = 2,
};

struct RawTrackAddress
{
    int cylinder;
    int head;
};

struct RawPreservationIdentity
{
    std::string_view family_name;
    std::string_view extension;
    std::string_view provisional_status;
};

struct RawPreservationMetadataSummary
{
    RawPreservationTier tier;
    std::size_t track_count;
    bool contains_capture_metadata;
    bool contains_timing_metadata;
};

struct RawPreservationIntegrityInfo
{
    RawIntegrityKind kind;
    bool per_track;
    bool whole_container;
};
}
