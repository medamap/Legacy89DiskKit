#include "legacy89diskkit/cpp/application/legacy89diskkit_application.hpp"
#include <iostream>
#include <vector>
#include <string>
#include <iomanip>
#include <algorithm>

using namespace legacy89diskkit::cpp;
using namespace legacy89diskkit::cpp::application;

void PrintUsage()
{
    std::cout << "Usage:" << std::endl;
    std::cout << "  ldk-verify <image_path>                      - Verify disk image" << std::endl;
    std::cout << "  ldk-verify create-boot <src> <dest> <files>  - Create bootable disk" << std::endl;
    std::cout << "  ldk-verify dump-files <image_path>           - Display file list" << std::endl;
}

void VerifyDisk(const std::string& path)
{
    std::cout << "==================================================" << std::endl;
    std::cout << "[TEST] Verifying: " << path << std::endl;
    std::cout << "==================================================" << std::endl;

    auto disk_service = CreateDiskService();
    auto status = disk_service.OpenDisk(path, true);
    if (!status.ok())
    {
        std::cerr << "[FATAL ERROR] Failed to open disk: " << status.message << std::endl;
        return;
    }

    auto session = disk_service.GetSession();
    auto metadata = disk_service.GetContainerMetadata();
    auto fs_info = session->GetFileSystemInfo();

    std::cout << "[DETECTION]" << std::endl;
    if (metadata)
    {
        std::cout << "  Container Format: " << metadata->image_format << std::endl;
        std::cout << "  Disk Type: " << static_cast<int>(metadata->disk_type) << std::endl;
        std::cout << "  Geometry: " << metadata->geometry.cylinders << "/" << metadata->geometry.heads 
                  << "/" << metadata->geometry.sectors_per_track << "/" << metadata->geometry.bytes_per_sector << std::endl;
    }
    std::cout << "  File System: " << fs_info.file_system_name << std::endl;
    std::cout << "  Platform: " << fs_info.platform_id << std::endl;

    std::cout << "\n[BOOT AREA]" << std::endl;
    auto boot_result = session->ReadBootArea();
    if (boot_result.ok())
    {
        const auto& boot_data = boot_result.value();
        std::cout << "  Size: " << boot_data.size() << " bytes" << std::endl;
        std::cout << "  Hex (16B): ";
        for (size_t i = 0; i < std::min<size_t>(16, boot_data.size()); ++i)
        {
            std::cout << std::hex << std::setw(2) << std::setfill('0') << static_cast<int>(boot_data[i]) << " ";
        }
        std::cout << std::dec << std::endl;
    }
    else
    {
        std::cout << "  [ERROR] Failed to read boot area: " << boot_result.status().message << std::endl;
    }

    std::cout << "\n[FILES]" << std::endl;
    auto files = session->GetFiles();
    std::cout << "  Count: " << files.size() << std::endl;
    std::cout << std::setfill(' ');
    std::cout << "  " << std::left << std::setw(24) << "Filename" << " | " 
              << std::setw(8) << "Size" << " | "
              << std::setw(12) << "Attributes" << " | "
              << "Type" << std::endl;
    std::cout << "  " << std::string(24, '-') << "-+-" << std::string(8, '-') << "-+-" << std::string(12, '-') << "-+-" << std::string(4, '-') << std::endl;

    for (size_t i = 0; i < std::min<size_t>(20, files.size()); ++i)
    {
        const auto& file = files[i];
        std::string attr_str = (file.attributes & 0x40) ? "R" : "."; // Simple R check
        attr_str += (file.attributes & 0x10) ? "H" : ".";
        
        std::string full_name = file.file_name;
        if (!file.extension.empty()) full_name += "." + file.extension;

        std::cout << "  " << std::left << std::setw(24) << full_name << " | "
                  << std::right << std::setw(8) << file.size << " | "
                  << std::left << std::setw(12) << attr_str << " | "
                  << ((file.attributes & 0x0c) ? "ASC" : "BIN") << std::endl;
    }

    if (files.size() > 20)
    {
        std::cout << "  ... (and " << files.size() - 20 << " more)" << std::endl;
    }

    std::cout << "\n[RESULT] Verification completed successfully." << std::endl;
}

void DumpFiles(const std::string& path)
{
    std::cout << "==================================================" << std::endl;
    std::cout << "[FILES] " << path << std::endl;
    std::cout << "==================================================" << std::endl;

    auto disk_service = CreateDiskService();
    auto status = disk_service.OpenDisk(path, true);
    if (!status.ok()) { std::cerr << "Error: " << status.message << std::endl; return; }

    auto session = disk_service.GetSession();
    auto files = session->GetFiles();
    std::string fs_name(session->FileSystemName());

    std::cout << "Count: " << files.size() << std::endl;
    std::cout << std::setfill(' ');
    std::cout << std::left << std::setw(24) << "Name" << " | "
              << std::setw(6) << "Attr" << " | "
              << std::setw(8) << "Size" << " | "
              << std::setw(6) << "Load" << " | "
              << std::setw(6) << "Exec" << " | " << "SC" << std::endl;
    std::cout << std::string(65, '-') << std::endl;

    for (const auto& file : files)
    {
        std::string attr_str;
        if (fs_name == "Hu-BASIC" || fs_name == "N88-BASIC")
        {
            attr_str += (file.attributes & 0x40) ? "R" : ".";
            attr_str += (file.attributes & 0x10) ? "H" : ".";
        }
        else if (fs_name == "MSX-DOS")
        {
            attr_str += (file.attributes & 0x01) ? "R" : ".";
            attr_str += (file.attributes & 0x02) ? "H" : ".";
        }

        std::string full_name = file.file_name;
        if (!file.extension.empty()) full_name += "." + file.extension;

        std::cout << std::left << std::setw(24) << full_name << " | "
                  << std::left << std::setw(6) << attr_str << " | "
                  << std::right << std::setw(8) << file.size << " | "
                  << "0x" << std::hex << std::setw(4) << std::setfill('0') << file.load_address << " | "
                  << "0x" << std::setw(4) << file.execution_address << std::dec << std::setfill(' ') << " | "
                  << "---" << std::endl; // SC not in NativeBridgeFileEntry
    }
}

void CreateBootDisk(const std::string& src_path, const std::string& dest_path, const std::vector<std::string>& files_to_copy)
{
    std::cout << "==================================================" << std::endl;
    std::cout << "[CREATE BOOT] Source: " << src_path << std::endl;
    std::cout << "[CREATE BOOT] Target: " << dest_path << std::endl;
    std::cout << "==================================================" << std::endl;

    auto disk_service = CreateDiskService();
    auto resolver = CreateExplicitFileSystemResolver();
    auto clone_service = CreateBootAndCloneService();

    // 1. Open Source
    auto src_status = disk_service.OpenDisk(src_path, true);
    if (!src_status.ok()) { std::cerr << "Failed to open source: " << src_status.message << std::endl; return; }
    auto src_session = disk_service.GetSession();
    auto src_metadata = disk_service.GetContainerMetadata();

    // 2. Create Target with same geometry
    std::cout << "[STEP 1] Creating blank disk: " << dest_path << std::endl;
    auto create_status = NativeFileSystemSession::Create(dest_path, src_metadata->disk_type, "BOOT_DISK");
    if (!create_status.ok()) { std::cerr << "Failed to create target: " << create_status.status().message << std::endl; return; }
    auto dest_session = std::move(create_status.value());

    // 3. Clone Boot Area and Track 0
    std::cout << "[STEP 2] Cloning Boot Area (IPL)..." << std::endl;
    auto boot_data = src_session->ReadBootArea();
    if (boot_data.ok())
    {
        dest_session.WriteBootArea(boot_data.value());
    }

    // 4. Initialize for detection
    std::cout << "[STEP 3] Initializing Target File System..." << std::endl;
    resolver.InitializeForDetection(dest_session);
    dest_session.Format();
    
    if (dest_session.FileSystemName() != src_session->FileSystemName())
    {
        std::cout << "  [WARNING] Target file system detected as " << dest_session.FileSystemName() 
                  << " but source was " << src_session->FileSystemName() << "." << std::endl;
        std::cout << "            This may be due to geometry differences in the blank disk template." << std::endl;
    }
    else
    {
        std::cout << "  Detected: " << dest_session.FileSystemName() << std::endl;
    }

    // 5. Transfer Files
    std::cout << "[STEP 4] Transferring Files..." << std::endl;
    clone_service.TransferFiles(src_session, &dest_session, files_to_copy);

    std::cout << "\n[RESULT] Boot disk created successfully: " << dest_path << std::endl;
}

int main(int argc, char* argv[])
{
    if (argc < 2)
    {
        PrintUsage();
        return 1;
    }

    std::string cmd = argv[1];
    if (argc >= 5 && cmd == "create-boot")
    {
        std::string src_path = argv[2];
        std::string dest_path = argv[3];
        std::string files_arg = argv[4];
        
        std::vector<std::string> files;
        std::size_t start = 0;
        std::size_t end = files_arg.find(',');
        while (end != std::string::npos)
        {
            files.push_back(files_arg.substr(start, end - start));
            start = end + 1;
            end = files_arg.find(',', start);
        }
        files.push_back(files_arg.substr(start));
        
        try {
            CreateBootDisk(src_path, dest_path, files);
        } catch (const std::exception& e) {
            std::cerr << "[FATAL ERROR] " << e.what() << std::endl;
            return 1;
        }
    }
    else if (cmd == "dump-files")
    {
        if (argc < 3) { PrintUsage(); return 1; }
        DumpFiles(argv[2]);
    }
    else
    {
        VerifyDisk(cmd);
    }

    return 0;
}
