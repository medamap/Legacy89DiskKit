#include "legacy89diskkit/cpp/application/disk_service.hpp"
#include "legacy89diskkit/cpp/application/directory_layout_service.hpp"
#include <iostream>
#include <vector>
#include <filesystem>
#include <fstream>
#include <sstream>
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
    TempFile disk_file(GetTempPath("directory_layout_smoke.d88"));

    // 1. Setup Disk
    DiskService disk_service;
    auto create_status = disk_service.CreateDisk(disk_file.string(), DiskType::TwoD, "LAYOUT_TEST");
    assert(create_status.ok());
    
    // Explicitly open as HuBasic to ensure layout support
    disk_service.CloseDisk();
    auto open_status = disk_service.OpenDisk(disk_file.string(), false, FileSystemFamily::HuBasic);
    assert(open_status.ok());

    auto format_status = disk_service.Format();
    assert(format_status.ok());

    auto session = disk_service.GetSession();
    std::cout << "FileSystem Name: " << session->FileSystemName() << std::endl;
    assert(session->FileSystemName() == "Hu-BASIC");

    // 2. Create some files
    std::vector<std::uint8_t> dummy_data(256, 0x00);
    session->WriteFile("FILE1.TXT", dummy_data, 0x01);
    session->WriteFile("FILE2.TXT", dummy_data, 0x01);
    session->WriteFile("FILE3.TXT", dummy_data, 0x01);

    DirectoryLayoutService layout_service(session);

    // 3. Export Plan
    std::cout << "Exporting Plan..." << std::endl;
    auto plan_result = layout_service.ExportPlan();
    if (!plan_result.ok())
    {
        std::cerr << "ExportPlan failed: " << plan_result.status().message << std::endl;
    }
    assert(plan_result.ok());
    std::string plan = plan_result.value();
    std::cout << "Original Plan:\n" << plan << std::endl;

    // 4. Modify Plan (Reverse order and add a label)
    std::stringstream ss(plan);
    std::vector<std::string> lines;
    std::string line;
    while (std::getline(ss, line))
    {
        if (!line.empty()) lines.push_back(line);
    }
    assert(lines.size() == 3);

    std::string new_plan = "# MY SECTION\n" + lines[2] + "\n" + lines[1] + "\n" + lines[0] + "\n";
    std::cout << "Applying New Plan:\n" << new_plan << std::endl;

    // 5. Apply Plan
    auto apply_result = layout_service.ApplyPlan(new_plan);
    if (!apply_result.ok())
    {
        std::cerr << "Apply failed: " << apply_result.status().message << std::endl;
    }
    assert(apply_result.ok());
    assert(apply_result.value().is_valid);

    // 6. Verify Result
    auto final_layout_result = layout_service.GetLayout();
    assert(final_layout_result.ok());
    auto final_layout = final_layout_result.value();
    
    // Should have 4 items: 1 label + 3 files
    assert(final_layout.items.size() == 4);
    assert(final_layout.items[0].kind == DirectoryLayoutItemKind::VirtualLabel);
    assert(final_layout.items[0].display_name == "MY SECTION");
    assert(final_layout.items[1].display_name == "FILE3.TXT");
    assert(final_layout.items[2].display_name == "FILE2.TXT");
    assert(final_layout.items[3].display_name == "FILE1.TXT");

    // 7. Individual Operations
    std::cout << "Testing MoveEntryBefore..." << std::endl;
    auto move_result = layout_service.MoveEntryBefore("FILE1.TXT", "FILE3.TXT");
    assert(move_result.ok());
    auto moved_layout = move_result.value();
    // Label, FILE1, FILE3, FILE2
    assert(moved_layout.items[1].display_name == "FILE1.TXT");
    assert(moved_layout.items[2].display_name == "FILE3.TXT");

    std::cout << "Testing InsertLabelBefore..." << std::endl;
    auto label_result = layout_service.InsertLabelBefore("NEW LABEL", "FILE2.TXT");
    assert(label_result.ok());
    auto labeled_layout = label_result.value();
    // Label, FILE1, FILE3, NEW LABEL, FILE2
    assert(labeled_layout.items[3].display_name == "NEW LABEL");
    assert(labeled_layout.items[4].display_name == "FILE2.TXT");

    std::cout << "DirectoryLayoutService smoke tests passed!" << std::endl;
    return 0;
}
