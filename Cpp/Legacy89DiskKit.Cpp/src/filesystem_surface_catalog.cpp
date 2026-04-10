#include "legacy89diskkit/cpp/filesystem_surface_catalog.hpp"

namespace legacy89diskkit::cpp
{
const std::vector<FileSystemSurfaceEntry>& FileSystemSurfaceCatalog::GetEntries()
{
    static const std::vector<FileSystemSurfaceEntry> entries{
        FileSystemSurfaceEntry{
            FileSystemFamily::HuBasic,
            "Hu-BASIC",
            SurfaceCapabilityList |
                SurfaceCapabilityFind |
                SurfaceCapabilityRead |
                SurfaceCapabilityWritePlan |
                SurfaceCapabilityRenamePlan |
                SurfaceCapabilityDeletePlan |
                SurfaceCapabilityAttributeUpdatePlan |
                SurfaceCapabilityFormatSeed |
                SurfaceCapabilityFileSystemInfo },
        FileSystemSurfaceEntry{
            FileSystemFamily::N88Basic,
            "N88-BASIC",
            SurfaceCapabilityList |
                SurfaceCapabilityFind |
                SurfaceCapabilityRead |
                SurfaceCapabilityWritePlan |
                SurfaceCapabilityRenamePlan |
                SurfaceCapabilityDeletePlan |
                SurfaceCapabilityAttributeUpdatePlan |
                SurfaceCapabilityFormatSeed |
                SurfaceCapabilityFileSystemInfo },
        FileSystemSurfaceEntry{
            FileSystemFamily::MsxDos,
            "MSX-DOS",
            SurfaceCapabilityList |
                SurfaceCapabilityFind |
                SurfaceCapabilityRead |
                SurfaceCapabilityWritePlan |
                SurfaceCapabilityRenamePlan |
                SurfaceCapabilityDeletePlan |
                SurfaceCapabilityAttributeUpdatePlan |
                SurfaceCapabilityFormatSeed |
                SurfaceCapabilityFileSystemInfo },
    };
    return entries;
}

const FileSystemSurfaceEntry* FileSystemSurfaceCatalog::FindByFamily(const FileSystemFamily family)
{
    for (const auto& entry : GetEntries())
    {
        if (entry.family == family)
        {
            return &entry;
        }
    }

    return nullptr;
}

bool FileSystemSurfaceCatalog::Supports(const FileSystemFamily family, const std::uint32_t capabilities)
{
    const auto* entry = FindByFamily(family);
    if (entry == nullptr)
    {
        return false;
    }

    return (entry->capabilities & capabilities) == capabilities;
}
}
