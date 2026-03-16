#include "legacy89diskkit/cpp/infrastructure/filesystem/filesystem_detection.hpp"

#include <algorithm>
#include <string>

namespace legacy89diskkit::cpp
{
namespace
{
template <typename TContainer>
bool IsHuBasicCandidate(const TContainer& container)
{
    const auto boot = container.ReadSector(0, 0, 1);
    if (!boot.ok() || boot.value().size() < 32)
    {
        return false;
    }

    const auto& data = boot.value();
    const auto has_boot_flag = data[0] == 0x01;
    const auto has_sys_extension = data[0x0e] == 'S' && data[0x0f] == 'y' && data[0x10] == 's';
    if (has_boot_flag && has_sys_extension)
    {
        return true;
    }

    const std::string label(data.begin(), data.begin() + 32);
    return label.find("BASIC") != std::string::npos;
}

template <typename TContainer>
bool IsN88BasicCandidate(const TContainer& container)
{
    if (container.DiskTypeValue() == DiskType::HardDisk)
    {
        return false;
    }

    const auto id_2d = container.ReadSector(9, 1, 13);
    if (id_2d.ok())
    {
        const auto dir_2d = container.ReadSector(9, 1, 1);
        if (dir_2d.ok() && dir_2d.value().size() >= 16)
        {
            const auto mode = dir_2d.value()[0];
            if (mode == 0x01 || mode == 0x00 || mode == 0xff)
            {
                return true;
            }
        }
    }

    const auto id_2dd = container.ReadSector(20, 0, 13);
    return id_2dd.ok();
}

template <typename TContainer>
bool IsMsxDosCandidate(const TContainer& container)
{
    const auto boot = container.ReadSector(0, 0, 1);
    if (boot.ok() && !boot.value().empty() && (boot.value()[0] == 0xeb || boot.value()[0] == 0xe9))
    {
        return true;
    }

    const auto fat = container.ReadSector(0, 0, 2);
    return fat.ok() && !fat.value().empty() && fat.value()[0] >= 0xf8;
}

std::vector<FileSystemDetectionCandidate> SortCandidates(std::vector<FileSystemDetectionCandidate> candidates)
{
    std::sort(
        candidates.begin(),
        candidates.end(),
        [](const auto& left, const auto& right)
        {
            if (left.score != right.score)
            {
                return left.score > right.score;
            }

            return static_cast<int>(left.family) < static_cast<int>(right.family);
        });
    return candidates;
}

template <typename TContainer>
std::vector<FileSystemDetectionCandidate> DetectCandidatesCore(const TContainer& container)
{
    std::vector<FileSystemDetectionCandidate> candidates;

    if (IsHuBasicCandidate(container))
    {
        candidates.push_back(FileSystemDetectionCandidate{
            FileSystemFamily::HuBasic,
            FileSystemSurfaceCatalog::FindByFamily(FileSystemFamily::HuBasic)->name,
            300});
    }

    if (IsN88BasicCandidate(container))
    {
        candidates.push_back(FileSystemDetectionCandidate{
            FileSystemFamily::N88Basic,
            FileSystemSurfaceCatalog::FindByFamily(FileSystemFamily::N88Basic)->name,
            200});
    }

    if (IsMsxDosCandidate(container))
    {
        candidates.push_back(FileSystemDetectionCandidate{
            FileSystemFamily::MsxDos,
            FileSystemSurfaceCatalog::FindByFamily(FileSystemFamily::MsxDos)->name,
            250});
    }

    return SortCandidates(std::move(candidates));
}
}

std::vector<FileSystemDetectionCandidate> FileSystemDetection::DetectCandidates(const RawDiskContainer& container)
{
    return DetectCandidatesCore(container);
}

std::vector<FileSystemDetectionCandidate> FileSystemDetection::DetectCandidates(const D88DiskContainer& container)
{
    return DetectCandidatesCore(container);
}

const FileSystemDetectionCandidate* FileSystemDetection::DetectBest(const RawDiskContainer& container)
{
    static std::vector<FileSystemDetectionCandidate> candidates;
    candidates = DetectCandidates(container);
    return candidates.empty() ? nullptr : &candidates.front();
}

const FileSystemDetectionCandidate* FileSystemDetection::DetectBest(const D88DiskContainer& container)
{
    static std::vector<FileSystemDetectionCandidate> candidates;
    candidates = DetectCandidates(container);
    return candidates.empty() ? nullptr : &candidates.front();
}
}
