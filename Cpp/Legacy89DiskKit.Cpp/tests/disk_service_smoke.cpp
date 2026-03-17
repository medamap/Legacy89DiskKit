#include "legacy89diskkit/cpp/application/disk_service.hpp"
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
    operator std::filesystem::path() const { return path; }
};

std::vector<std::uint8_t> CreateHuBasicImage()
{
    std::vector<std::uint8_t> image(327680, 0x00);
    image[0] = 0x01;
    image[0x0e] = 'S';
    image[0x0f] = 'y';
    image[0x10] = 's';
    return image;
}

std::filesystem::path GetTempPath(const std::string& filename)
{
    return std::filesystem::temp_directory_path() / filename;
}
}

int main()
{
    TempFile image_file(GetTempPath("disk_service_smoke_test.img"));
    TempFile new_d88_file(GetTempPath("disk_service_smoke_new.d88"));
    TempFile ghost_file(GetTempPath("disk_service_smoke_ghost.img"));
    
    const auto image_path = image_file.path;
    const auto new_d88_path = new_d88_file.path;
    const auto ghost_path = ghost_file.path;

    // Setup valid image
    {
        std::ofstream stream(image_path, std::ios::binary);
        const auto image = CreateHuBasicImage();
        stream.write(reinterpret_cast<const char*>(image.data()), static_cast<std::streamsize>(image.size()));
    }

    DiskService service;

    // --- Path-based Open ---
    {
        auto status = service.OpenDisk(image_path.string(), true);
        assert(status.ok());
        assert(service.IsDiskOpen());
        assert(service.GetSession() != nullptr);
        
        auto metadata = service.GetContainerMetadata();
        assert(metadata.has_value());
        assert(metadata->image_format == "raw-sector-container");

        service.CloseDisk();
        assert(!service.IsDiskOpen());
    }

    // --- Buffer-based Open ---
    {
        const auto image = CreateHuBasicImage();
        auto status = service.OpenDiskFromBuffer(image, true, BufferDiskImageFormat::Raw);
        assert(status.ok());
        assert(service.IsDiskOpen());
        
        auto metadata = service.GetContainerMetadata();
        assert(metadata.has_value());
        assert(metadata->image_format == "raw-sector-container");
    }

    // --- Persistence and Flow Verification ---
    {
        // 1. Create a new disk
        std::cout << "Testing Disk Creation and Persistence..." << std::endl;
        auto status = service.CreateDisk(new_d88_path.string(), DiskType::TwoD, "PERSIST_TEST");
        assert(status.ok());
        assert(service.IsDiskOpen());
        
        // Verify file was actually created on disk
        assert(std::filesystem::exists(new_d88_path));
        assert(std::filesystem::file_size(new_d88_path) > 0);
        
        // Explicitly verify D88 header to ensure persistence parity (not just in-memory)
        {
            std::ifstream check_stream(new_d88_path, std::ios::binary);
            char header[17] = {0};
            check_stream.read(header, 17);
            std::string header_name(header);
            assert(header_name == "PERSIST_TEST");
        }

        // 2. Format it (this should also update the file on disk)
        std::cout << "Formatting..." << std::endl;
        auto format_status = service.Format();
        assert(format_status.ok());
        
        auto fs_info = service.GetSession()->GetFileSystemInfo();
        std::cout << "FileSystem Name: [" << fs_info.file_system_name << "]" << std::endl;
        assert(fs_info.file_system_name == "Hu-BASIC" || fs_info.file_system_name == "N88-BASIC");

        // 3. Close the disk
        service.CloseDisk();
        assert(!service.IsDiskOpen());

        // 4. Re-open from path and verify contents/format still there
        std::cout << "Testing Re-open Parity..." << std::endl;
        auto reopen_status = service.OpenDisk(new_d88_path.string(), false);
        assert(reopen_status.ok());
        assert(service.IsDiskOpen());
        
        auto reopen_fs_info = service.GetSession()->GetFileSystemInfo();
        std::cout << "Reopened FileSystem Name: [" << reopen_fs_info.file_system_name << "]" << std::endl;
        assert(reopen_fs_info.file_system_name == "Hu-BASIC" || reopen_fs_info.file_system_name == "N88-BASIC");
        
        auto metadata = service.GetContainerMetadata();
        assert(metadata->image_format == "d88-sector-container");

        // 5. Test WriteFile persistence
        std::cout << "Testing WriteFile Persistence..." << std::endl;
        std::vector<std::uint8_t> test_data(256, 0xAA);
        test_data[0] = 0xDE;
        test_data[1] = 0xAD;
        test_data[2] = 0xBE;
        test_data[3] = 0xEF;
        auto write_status = service.GetSession()->WriteFile("TEST.BIN", test_data);
        assert(write_status.ok());

        std::cout << "Testing Explicit Save..." << std::endl;
        auto save_status = service.Save();
        assert(save_status.ok());

        service.CloseDisk();

        // Re-open and verify file exists
        auto reopen_write_status = service.OpenDisk(new_d88_path.string(), true);
        assert(reopen_write_status.ok());
        
        assert(service.GetSession()->FileExists("TEST.BIN"));
        auto read_result = service.GetSession()->ReadFile("TEST.BIN");
        assert(read_result.ok());
        assert(read_result.value() == test_data);

        service.CloseDisk();
    }

    // --- Failure Case Handling ---
    {
        // 1. Invalid path
        auto status = service.OpenDisk(ghost_path.string(), true);
        assert(!status.ok());
        assert(!service.IsDiskOpen());

        // 2. Success -> Failure -> Closed state check
        service.OpenDisk(image_path.string(), true);
        assert(service.IsDiskOpen());

        std::vector<std::uint8_t> bad_data(512, 0x00);
        service.OpenDiskFromBuffer(bad_data, true);
        assert(!service.IsDiskOpen()); 
    }

    std::cout << "DiskService refined smoke tests passed!" << std::endl;
    return 0;
}

