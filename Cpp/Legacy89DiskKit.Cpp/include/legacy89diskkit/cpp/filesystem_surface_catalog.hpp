#pragma once

#include <cstdint>
#include <string_view>
#include <vector>

namespace legacy89diskkit::cpp
{
enum class FileSystemFamily
{
    HuBasic,
    N88Basic,
    MsxDos,
};

enum FileSystemSurfaceCapabilities : std::uint32_t
{
    SurfaceCapabilityList = 1u << 0,
    SurfaceCapabilityFind = 1u << 1,
    SurfaceCapabilityRead = 1u << 2,
    SurfaceCapabilityWritePlan = 1u << 3,
    SurfaceCapabilityRenamePlan = 1u << 4,
    SurfaceCapabilityDeletePlan = 1u << 5,
    SurfaceCapabilityAttributeUpdatePlan = 1u << 6,
    SurfaceCapabilityFormatSeed = 1u << 7,
    SurfaceCapabilityFileSystemInfo = 1u << 8,
};

struct FileSystemSurfaceEntry
{
    FileSystemFamily family;
    std::string_view name;
    std::uint32_t capabilities;
};

class FileSystemSurfaceCatalog
{
public:
    static const std::vector<FileSystemSurfaceEntry>& GetEntries();
    static const FileSystemSurfaceEntry* FindByFamily(FileSystemFamily family);
    static bool Supports(FileSystemFamily family, std::uint32_t capabilities);
};
}
