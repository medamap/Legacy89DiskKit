#pragma once

#include "legacy89diskkit/cpp/filesystem_surface_catalog.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"

#include <vector>

namespace legacy89diskkit::cpp
{
struct FileSystemDetectionCandidate
{
    FileSystemFamily family;
    std::string_view canonical_name;
    int score;
};

class FileSystemDetection
{
public:
    static std::vector<FileSystemDetectionCandidate> DetectCandidates(const RawDiskContainer& container);
    static std::vector<FileSystemDetectionCandidate> DetectCandidates(const D88DiskContainer& container);
    static const FileSystemDetectionCandidate* DetectBest(const RawDiskContainer& container);
    static const FileSystemDetectionCandidate* DetectBest(const D88DiskContainer& container);
};
}
