#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/msx_dos/msx_dos_file_system.hpp"
#include "legacy89diskkit/cpp/msx_dos_boot_sector.hpp"
#include "legacy89diskkit/cpp/msx_dos_boot_sector_parser.hpp"
#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

#include <algorithm>
#include <vector>

using namespace legacy89diskkit::cpp;

namespace
{
std::vector<std::uint8_t> CreateRawTwoDDImage()
{
    return std::vector<std::uint8_t>(737280, 0x00);
}
}

int main()
{
    auto container_result = RawDiskContainer::OpenFromBuffer(CreateRawTwoDDImage(), false);
    if (!container_result.ok())
    {
        return 1;
    }

    auto& container = container_result.value();

    MsxDosBootSector boot_sector{
        {0xeb, 0xfe, 0x90},
        {'M', 'S', 'X', 'D', 'O', 'S', ' ', ' '},
        MsxDosConfigurationProvider::GetDefault(DiskType::TwoDD)
    };
    const auto boot_bytes = MsxDosBootSectorParser::Write(boot_sector);
    if (!container.WriteSector(0, 0, 1, boot_bytes).ok())
    {
        return 2;
    }

    auto file_system_result = MsxDosFileSystem::Open(container);
    if (!file_system_result.ok())
    {
        return 3;
    }

    auto& file_system = file_system_result.value();
    const auto format_status = file_system.Format();
    if (!format_status.ok())
    {
        return 4;
    }

    const auto initial_info = file_system.GetFileSystemInfo();
    if (initial_info.cluster_size != 1024 || initial_info.free_space <= 0)
    {
        return 5;
    }

    const MsxDosFileAttributes binary_attributes{false, 0x00, false, false, false, false, false};
    const std::vector<std::uint8_t> payload{0x10, 0x20, 0x30, 0x40};
    const auto write_status = file_system.WriteFile("TEST.BIN", payload, binary_attributes);
    if (!write_status.ok())
    {
        return 6;
    }

    const auto files_after_write = file_system.GetFiles();
    if (files_after_write.size() != 1 || !file_system.FileExists("TEST.BIN"))
    {
        return 7;
    }

    const auto read_result = file_system.ReadFile("TEST.BIN");
    if (!read_result.ok() || read_result.value() != payload)
    {
        return 8;
    }

    const auto rename_status = file_system.RenameFile("TEST.BIN", "RENAMED.BIN");
    if (!rename_status.ok() || !file_system.FileExists("RENAMED.BIN") || file_system.FileExists("TEST.BIN"))
    {
        return 9;
    }

    const MsxDosFileAttributes read_only_attributes{false, 0x01, true, false, false, false, false};
    const auto update_status = file_system.UpdateAttributes("RENAMED.BIN", read_only_attributes);
    if (!update_status.ok())
    {
        return 10;
    }

    const auto files_after_update = file_system.GetFiles();
    if (files_after_update.size() != 1 || !files_after_update[0].attributes.is_read_only)
    {
        return 11;
    }

    const auto delete_status = file_system.DeleteFile("RENAMED.BIN");
    if (!delete_status.ok() || file_system.FileExists("RENAMED.BIN") || !file_system.GetFiles().empty())
    {
        return 12;
    }

    const auto boot_read = file_system.ReadBootArea();
    if (!boot_read.ok() || boot_read.value().size() != 512)
    {
        return 13;
    }

    return 0;
}
