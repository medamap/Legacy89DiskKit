#include "legacy89diskkit/cpp/application/disk_service.hpp"
#include "legacy89diskkit/cpp/application/boot_and_clone_service.hpp"
#include <iostream>
#include <vector>
#include <filesystem>
#include <fstream>
#include <cassert>
#include <algorithm>

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
    TempFile source_file(GetTempPath("boot_clone_source.d88"));
    TempFile target_file(GetTempPath("boot_clone_target.d88"));

    // 1. Setup Source Disk (HuBasic)
    DiskService source_service;
    assert(source_service.CreateDisk(source_file.string(), DiskType::TwoD, "SOURCE").ok());
    assert(source_service.OpenDisk(source_file.string(), false, FileSystemFamily::HuBasic).ok());
    assert(source_service.Format().ok());
    auto source_session = source_service.GetSession();

    // 2. Setup Target Disk (HuBasic)
    DiskService target_service;
    assert(target_service.CreateDisk(target_file.string(), DiskType::TwoD, "TARGET").ok());
    assert(target_service.OpenDisk(target_file.string(), false, FileSystemFamily::HuBasic).ok());
    assert(target_service.Format().ok());
    auto target_session = target_service.GetSession();

    BootAndCloneService service;

    // --- Transfer Boot Area Test ---
    {
        std::cout << "Testing TransferBootArea..." << std::endl;
        std::vector<std::uint8_t> mock_boot(256, 0xAA);
        mock_boot[0] = 0x01; // Magic
        mock_boot[1] = 'I';
        mock_boot[2] = 'P';
        mock_boot[3] = 'L';
        
        assert(source_session->WriteBootArea(mock_boot).ok());

        auto status = service.TransferBootArea(source_session, target_session);
        assert(status.ok());

        auto target_boot = target_session->ReadBootArea();
        assert(target_boot.ok());
        assert(target_boot.value()[0] == 0x01);
        assert(target_boot.value()[1] == 'I');
        assert(target_boot.value()[3] == 'L');
    }

    // --- Transfer Files Test ---
    {
        std::cout << "Testing TransferFiles with Addresses..." << std::endl;
        std::vector<std::uint8_t> file_data = {0xDE, 0xAD, 0xBE, 0xEF};
        // HuBasic WriteFile takes addresses
        assert(source_session->WriteFile("TEST.BIN", file_data, 0x00, 0x1234, 0x5678).ok());

        auto status = service.TransferFiles(source_session, target_session, {"TEST.BIN"});
        assert(status.ok());

        assert(target_session->FileExists("TEST.BIN"));
        auto files = target_session->GetFiles();
        auto it = std::find_if(files.begin(), files.end(), [](const auto& f) { return f.file_name == "TEST"; });
        assert(it != files.end());
        
        // Verify address preservation
        std::cout << "Preserved Load Address: 0x" << std::hex << it->load_address << std::dec << std::endl;
        assert(it->load_address == 0x1234);
        assert(it->execution_address == 0x5678);
    }

    // --- Boot Info Summary Test ---
    {
        std::cout << "Testing GetBootInfoSummary..." << std::endl;
        
        // Ensure target boot area is actually empty for "None" test
        std::vector<std::uint8_t> empty_boot(256, 0x00);
        assert(target_session->WriteBootArea(empty_boot).ok());

        auto target_summary = service.GetBootInfoSummary(target_session);
        assert(target_summary.ok());
        if (target_summary.value().mode != BootInfoMode::None)
        {
            std::cout << "DEBUG: Target Boot Mode is " << static_cast<int>(target_summary.value().mode) << " but expected None(0)" << std::endl;
        }
        assert(target_summary.value().mode == BootInfoMode::None);

        // Source has mock boot area (non-zero), should be SectorResident or FileBacked
        auto source_summary = service.GetBootInfoSummary(source_session);
        assert(source_summary.ok());
        assert(source_summary.value().mode != BootInfoMode::None);
        
        std::cout << "Target Boot Mode: " << static_cast<int>(target_summary.value().mode) << " (Expected 0 for None)" << std::endl;
        std::cout << "Source Boot Mode: " << static_cast<int>(source_summary.value().mode) << std::endl;
    }

    std::cout << "BootAndCloneService smoke tests passed!" << std::endl;
    return 0;
}
