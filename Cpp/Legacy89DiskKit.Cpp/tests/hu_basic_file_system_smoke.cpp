#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/hu_basic/hu_basic_file_system.hpp"

#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateRawTwoDImage()
{
    return std::vector<std::uint8_t>(327680, 0x00);
}
}

int main()
{
    auto container_result = RawDiskContainer::OpenFromBuffer(CreateRawTwoDImage(), false);
    if (!container_result.ok())
    {
        return 1;
    }

    auto& container = container_result.value();
    auto file_system = HuBasicFileSystem::Open(container);

    const auto format_status = file_system.Format();
    if (!format_status.ok())
    {
        return 2;
    }

    const auto initial_info = file_system.GetFileSystemInfo();
    if (initial_info.cluster_size != 4096 || initial_info.free_space <= 0)
    {
        return 3;
    }

    const HuBasicFileAttributes binary_attributes{false, 0x00, false, false, false};
    const std::vector<std::uint8_t> payload{0x10, 0x20, 0x30, 0x40};
    const auto write_status = file_system.WriteFile("TEST.BIN", payload, binary_attributes, 0x4000, 0x4000);
    if (!write_status.ok())
    {
        return 4;
    }

    const auto files_after_write = file_system.GetFiles();
    if (files_after_write.size() != 1 || !file_system.FileExists("TEST.BIN"))
    {
        return 5;
    }

    const auto read_result = file_system.ReadFile("TEST.BIN");
    if (!read_result.ok() || read_result.value() != payload)
    {
        return 6;
    }

    const auto rename_status = file_system.RenameFile("TEST.BIN", "RENAMED.BIN");
    if (!rename_status.ok() || !file_system.FileExists("RENAMED.BIN") || file_system.FileExists("TEST.BIN"))
    {
        return 7;
    }

    const HuBasicFileAttributes read_only_attributes{false, 0x00, false, true, false};
    const auto update_status = file_system.UpdateAttributes("RENAMED.BIN", read_only_attributes);
    if (!update_status.ok())
    {
        return 8;
    }

    const auto files_after_update = file_system.GetFiles();
    if (files_after_update.size() != 1 || !files_after_update[0].attributes.is_read_only)
    {
        return 9;
    }

    const auto delete_status = file_system.DeleteFile("RENAMED.BIN");
    if (!delete_status.ok() || file_system.FileExists("RENAMED.BIN") || !file_system.GetFiles().empty())
    {
        return 10;
    }

    return 0;
}
