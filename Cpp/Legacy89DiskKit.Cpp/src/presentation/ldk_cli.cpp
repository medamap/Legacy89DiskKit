#include "legacy89diskkit/cpp/application/legacy89diskkit_application.hpp"
#include "legacy89diskkit/cpp/presentation/cli_localizer.hpp"
#include "legacy89diskkit/cpp/infrastructure/character_encoding/character_encoding_table_catalog.hpp"
#include <iostream>
#include <vector>
#include <string>
#include <iomanip>
#include <algorithm>
#include <fstream>
#include <filesystem>

using namespace legacy89diskkit::cpp;
using namespace legacy89diskkit::cpp::application;
using namespace legacy89diskkit::cpp::presentation;

struct CliOptions
{
    std::string language = "en";
    std::string encoding = "";
    std::string target_name = "";
    std::vector<std::string> args;
};

const CliLocalizer* g_localizer = &CliLocalizer::GetEn();

void PrintHelp()
{
    std::cout << g_localizer->Get(MessageKey::RootDescription) << std::endl;
    std::cout << "\nUsage:" << std::endl;
    std::cout << "  ldk list <image>                - " << g_localizer->Get(MessageKey::ListCommandDescription) << std::endl;
    std::cout << "  ldk file extract <image> <disk-file> <host-path>" << std::endl;
    std::cout << "  ldk file inject <image> <host-file> [-n <target-name>]" << std::endl;
    std::cout << "  ldk file delete <image> <disk-file>" << std::endl;
    std::cout << "  ldk file rename <image> <old-name> <new-name>" << std::endl;
    std::cout << "  ldk disk create <image> -d <type> -f <fs> [-n <name>]" << std::endl;
    std::cout << "  ldk disk format <image>" << std::endl;
    std::cout << "  ldk boot show <image>           - " << g_localizer->Get(MessageKey::BootShowCommandDescription) << std::endl;
    std::cout << "  ldk layout show <image>         - " << g_localizer->Get(MessageKey::LayoutShowCommandDescription) << std::endl;
    std::cout << "\nOptions:" << std::endl;
    std::cout << "  -l, --language <lang>           - " << g_localizer->Get(MessageKey::LanguageOptionDescription) << std::endl;
    std::cout << "  -e, --encoding <enc>            - " << g_localizer->Get(MessageKey::EncodingOptionDescription) << std::endl;
}

std::string DecodeName(const std::string& raw_name, const ByteTextEncodingTable* table)
{
    if (!table) return raw_name;
    ByteTextEncodingIndex index(*table);
    std::vector<uint8_t> bytes(raw_name.begin(), raw_name.end());
    return index.Decode(bytes, "");
}

const ByteTextEncodingTable* GetTable(const std::string& encoding_override, const std::string& platform_id)
{
    if (!encoding_override.empty())
    {
        auto table_result = CharacterEncodingTableCatalog::Find(encoding_override);
        if (table_result.ok()) return table_result.value();
    }
    auto table_result = CharacterEncodingTableCatalog::Find(platform_id);
    if (table_result.ok()) return table_result.value();
    return nullptr;
}

void ListFiles(const std::string& image_path, const std::string& encoding_override)
{
    auto disk_service = CreateDiskService();
    auto status = disk_service.OpenDisk(image_path, true);
    if (!status.ok()) { std::cerr << "Error: " << status.message << std::endl; return; }

    auto session = disk_service.GetSession();
    auto fs_info = session->GetFileSystemInfo();
    auto metadata = disk_service.GetContainerMetadata();
    auto* table = GetTable(encoding_override, fs_info.platform_id);

    std::cout << "Listing files for: " << image_path << std::endl;
    std::cout << g_localizer->Get(MessageKey::FileSystemLabel) << ": " << fs_info.file_system_name << std::endl;
    std::cout << g_localizer->Get(MessageKey::PlatformLabel) << ": " << fs_info.platform_id << std::endl;
    
    auto files = session->GetFiles();
    std::cout << g_localizer->Get(MessageKey::FileCountLabel) << ": " << files.size() << std::endl;
    
    if (metadata)
    {
        std::cout << g_localizer->Get(MessageKey::TotalCapacityLabel) << ": " << fs_info.total_capacity << std::endl;
        std::cout << g_localizer->Get(MessageKey::UsedSpaceLabel) << ": " << fs_info.total_capacity - fs_info.free_space << std::endl;
        std::cout << g_localizer->Get(MessageKey::FreeSpaceLabel) << ": " << fs_info.free_space << std::endl;
    }

    std::cout << "\n";
    std::cout << std::left << std::setw(24) << g_localizer->Get(MessageKey::FileNameHeader) << " | "
              << std::setw(6) << g_localizer->Get(MessageKey::TypeHeader) << " | "
              << std::setw(8) << g_localizer->Get(MessageKey::SizeHeader) << " | "
              << std::setw(6) << g_localizer->Get(MessageKey::LoadHeader) << " | "
              << std::setw(6) << g_localizer->Get(MessageKey::ExecHeader) << std::endl;
    std::cout << std::string(60, '-') << std::endl;

    for (const auto& file : files)
    {
        std::string full_name = DecodeName(file.file_name, table);
        if (!file.extension.empty()) full_name += "." + DecodeName(file.extension, table);

        std::cout << std::left << std::setw(24) << full_name << " | "
                  << std::setw(6) << ((file.attributes & 0x0c) ? "ASC" : "BIN") << " | "
                  << std::right << std::setw(8) << file.size << " | "
                  << "0x" << std::hex << std::setw(4) << std::setfill('0') << file.load_address << " | "
                  << "0x" << std::setw(4) << file.execution_address << std::dec << std::setfill(' ') << std::endl;
    }
}

void ExtractFile(const std::string& image_path, const std::string& disk_file, const std::string& host_path)
{
    auto disk_service = CreateDiskService();
    auto status = disk_service.OpenDisk(image_path, true);
    if (!status.ok()) { std::cerr << "Error: " << status.message << std::endl; return; }

    auto session = disk_service.GetSession();
    auto result = session->ReadFile(disk_file);
    if (!result.ok())
    {
        std::cerr << g_localizer->Get(MessageKey::ErrorFileNotFound) << disk_file << " (" << result.status().message << ")" << std::endl;
        return;
    }

    std::ofstream ofs(host_path, std::ios::binary);
    if (!ofs) { std::cerr << "Error: Could not open host path for writing: " << host_path << std::endl; return; }
    
    const auto& data = result.value();
    ofs.write(reinterpret_cast<const char*>(data.data()), data.size());
    std::cout << g_localizer->Get(MessageKey::FileExtractedMessage) << std::endl;
}

void InjectFile(const std::string& image_path, const std::string& host_path, const std::string& target_name)
{
    auto disk_service = CreateDiskService();
    auto status = disk_service.OpenDisk(image_path, false);
    if (!status.ok()) { std::cerr << "Error: " << status.message << std::endl; return; }

    std::ifstream ifs(host_path, std::ios::binary);
    if (!ifs) { std::cerr << "Error: Could not open host file for reading: " << host_path << std::endl; return; }

    std::vector<uint8_t> data((std::istreambuf_iterator<char>(ifs)), std::istreambuf_iterator<char>());
    auto session = disk_service.GetSession();
    auto write_status = session->WriteFile(target_name, data, 0x01, 0, 0);
    if (!write_status.ok()) { std::cerr << "Error: " << write_status.message << std::endl; return; }

    std::cout << g_localizer->Get(MessageKey::FileInjectedMessage) << std::endl;
}

int main(int argc, char* argv[])
{
    if (argc < 2) { PrintHelp(); return 1; }

    CliOptions options;
    int arg_idx = 1;
    while (arg_idx < argc)
    {
        std::string arg = argv[arg_idx];
        if (arg == "-l" || arg == "--language") { if (++arg_idx < argc) options.language = argv[arg_idx]; }
        else if (arg == "-e" || arg == "--encoding") { if (++arg_idx < argc) options.encoding = argv[arg_idx]; }
        else if (arg == "-n" || arg == "--name") { if (++arg_idx < argc) options.target_name = argv[arg_idx]; }
        else { options.args.push_back(arg); }
        arg_idx++;
    }

    if (options.language == "ja") g_localizer = &CliLocalizer::GetJa();
    else g_localizer = &CliLocalizer::GetEn();

    if (options.args.empty()) { PrintHelp(); return 1; }

    std::string cmd = options.args[0];
    if (cmd == "list")
    {
        if (options.args.size() < 2) { std::cerr << "Usage: ldk list <image>" << std::endl; return 1; }
        ListFiles(options.args[1], options.encoding);
    }
    else if (cmd == "file")
    {
        if (options.args.size() < 2) { PrintHelp(); return 1; }
        std::string sub = options.args[1];
        if (sub == "extract")
        {
            if (options.args.size() < 5) { std::cerr << "Usage: ldk file extract <image> <disk-file> <host-path>" << std::endl; return 1; }
            ExtractFile(options.args[2], options.args[3], options.args[4]);
        }
        else if (sub == "inject")
        {
            if (options.args.size() < 4) { std::cerr << "Usage: ldk file inject <image> <host-file> [-n <target-name>]" << std::endl; return 1; }
            std::string target = options.target_name.empty() ? std::filesystem::path(options.args[3]).filename().string() : options.target_name;
            InjectFile(options.args[2], options.args[3], target);
        }
        else if (sub == "delete")
        {
            if (options.args.size() < 4) { std::cerr << "Usage: ldk file delete <image> <disk-file>" << std::endl; return 1; }
            auto disk_service = CreateDiskService();
            if (disk_service.OpenDisk(options.args[2], false).ok())
            {
                if (disk_service.GetSession()->DeleteFile(options.args[3]).ok())
                    std::cout << g_localizer->Get(MessageKey::FileDeletedMessage) << std::endl;
            }
        }
        else if (sub == "rename")
        {
            if (options.args.size() < 5) { std::cerr << "Usage: ldk file rename <image> <old-name> <new-name>" << std::endl; return 1; }
            auto disk_service = CreateDiskService();
            if (disk_service.OpenDisk(options.args[2], false).ok())
            {
                if (disk_service.GetSession()->RenameFile(options.args[3], options.args[4]).ok())
                    std::cout << g_localizer->Get(MessageKey::FileRenamedMessage) << std::endl;
            }
        }
    }
    else if (cmd == "disk")
    {
        if (options.args.size() < 2) { PrintHelp(); return 1; }
        std::string sub = options.args[1];
        if (sub == "format")
        {
            if (options.args.size() < 3) { std::cerr << "Usage: ldk disk format <image>" << std::endl; return 1; }
            auto disk_service = CreateDiskService();
            if (disk_service.OpenDisk(options.args[2], false).ok())
            {
                if (disk_service.GetSession()->Format().ok())
                    std::cout << g_localizer->Get(MessageKey::DiskFormattedMessage) << std::endl;
            }
        }
        else if (sub == "create")
        {
            if (options.args.size() < 3) { std::cerr << "Usage: ldk disk create <image> -d <type> -f <fs>" << std::endl; return 1; }
            // For now, hardcode 2D/Hu-BASIC or parse more flags
            auto create_status = NativeFileSystemSession::Create(options.args[2], DiskType::TwoD, options.target_name);
            if (create_status.ok())
            {
                auto session = std::move(create_status.value());
                session.Format();
                std::cout << g_localizer->Get(MessageKey::DiskCreatedMessage) << std::endl;
            }
        }
    }
    else if (cmd == "boot" && options.args.size() >= 3 && options.args[1] == "show")
    {
        auto disk_service = CreateDiskService();
        if (disk_service.OpenDisk(options.args[2], true).ok())
        {
            auto session = disk_service.GetSession();
            auto info = session->GetFileSystemInfo();
            std::cout << g_localizer->Get(MessageKey::FileSystemLabel) << ": " << info.file_system_name << std::endl;
            auto result = session->ReadBootArea();
            if (result.ok()) std::cout << "Boot Area Size: " << result.value().size() << " bytes" << std::endl;
        }
    }
    else if (cmd == "layout" && options.args.size() >= 3 && options.args[1] == "show")
    {
        auto disk_service = CreateDiskService();
        if (disk_service.OpenDisk(options.args[2], true).ok())
        {
            auto session = disk_service.GetSession();
            auto result = session->ReadDirectoryLayout();
            if (result.ok())
            {
                for (const auto& item : result.value().items)
                {
                    std::cout << std::setw(3) << std::setfill('0') << item.order << " [" 
                              << (item.kind == DirectoryLayoutItemKind::FileEntry ? "FileEntry" : "Label") << "] "
                              << item.id << " " << DecodeName(item.display_name, nullptr) << std::endl;
                }
            }
        }
    }
    else { PrintHelp(); }

    return 0;
}
