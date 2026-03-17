#include "legacy89diskkit/cpp/application/legacy89diskkit_application.hpp"
#include <iostream>
#include <vector>
#include <filesystem>
#include <fstream>
#include <cassert>

using namespace legacy89diskkit::cpp;
using namespace legacy89diskkit::cpp::application;

namespace
{
struct TempFile
{
    std::filesystem::path path;
    explicit TempFile(std::filesystem::path p) : path(std::move(p)) 
    {
        if (std::filesystem::exists(path)) std::filesystem::remove(path);
    }
    ~TempFile()
    {
        if (std::filesystem::exists(path)) std::filesystem::remove(path);
    }
    std::string string() const { return path.string(); }
};

std::filesystem::path GetTempPath(const std::string& filename)
{
    return std::filesystem::temp_directory_path() / filename;
}
}

int main()
{
    TempFile disk_file(GetTempPath("bootstrap_smoke.d88"));

    // 1. Bootstrap Services
    auto disk_service = CreateDiskService();
    auto resolver = CreateExplicitFileSystemResolver();
    auto clone_service = CreateBootAndCloneService();

    // 2. Create and Format (explicitly HuBasic initially)
    std::cout << "Step 1: Create and Format as Hu-BASIC..." << std::endl;
    assert(disk_service.CreateDisk(disk_file.string(), DiskType::TwoD, "BOOTSTRAP_TEST").ok());
    assert(disk_service.OpenDisk(disk_file.string(), false, FileSystemFamily::HuBasic).ok());
    assert(disk_service.Format().ok());

    // 3. Initialize for Detection (Application Layer Orchestration)
    // This should write the magic byte (0x01) for Hu-BASIC.
    std::cout << "Step 2: Initialize for detection..." << std::endl;
    assert(resolver.InitializeForDetection(*disk_service.GetSession()).ok());
    
    // Save and Close
    disk_service.Save();
    disk_service.CloseDisk();

    // 4. Verify Automatic Detection (The core promise of InitializeForDetection)
    std::cout << "Step 3: Verify automatic detection..." << std::endl;
    auto open_status = disk_service.OpenDisk(disk_file.string(), true); // No explicit family
    if (!open_status.ok())
    {
        std::cerr << "Auto-detection failed: " << open_status.message << std::endl;
    }
    assert(open_status.ok());
    
    std::cout << "Detected FileSystem: " << disk_service.GetSession()->FileSystemName() << std::endl;
    assert(disk_service.GetSession()->Family() == FileSystemFamily::HuBasic);

    // 5. Verify other bootstrap factories
    std::cout << "Step 4: Verify other factories..." << std::endl;
    auto transfer_service = CreateFileTransferService(disk_service.GetSession());
    auto layout_service = CreateDirectoryLayoutService(disk_service.GetSession());

    // Check canonical names
    assert(ExplicitFileSystemResolver::GetCanonicalName(FileSystemFamily::HuBasic) == "Hu-BASIC");
    assert(ExplicitFileSystemResolver::GetCanonicalName(FileSystemFamily::N88Basic) == "N88-BASIC");

    std::cout << "Bootstrap smoke tests passed!" << std::endl;
    return 0;
}
