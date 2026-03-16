#include "legacy89diskkit/cpp/filesystem_surface_catalog.hpp"

using namespace legacy89diskkit::cpp;

int main()
{
    const auto& entries = FileSystemSurfaceCatalog::GetEntries();
    if (entries.size() != 3)
    {
        return 1;
    }

    const auto required_capabilities =
        SurfaceCapabilityList |
        SurfaceCapabilityFind |
        SurfaceCapabilityRead |
        SurfaceCapabilityWritePlan |
        SurfaceCapabilityRenamePlan |
        SurfaceCapabilityDeletePlan |
        SurfaceCapabilityAttributeUpdatePlan |
        SurfaceCapabilityFormatSeed |
        SurfaceCapabilityFileSystemInfo;

    if (!FileSystemSurfaceCatalog::Supports(FileSystemFamily::HuBasic, required_capabilities))
    {
        return 2;
    }

    if (!FileSystemSurfaceCatalog::Supports(FileSystemFamily::N88Basic, required_capabilities))
    {
        return 3;
    }

    if (!FileSystemSurfaceCatalog::Supports(FileSystemFamily::MsxDos, required_capabilities))
    {
        return 4;
    }

    const auto* msx_entry = FileSystemSurfaceCatalog::FindByFamily(FileSystemFamily::MsxDos);
    if (msx_entry == nullptr || msx_entry->name != "MSX-DOS")
    {
        return 5;
    }

    return 0;
}
