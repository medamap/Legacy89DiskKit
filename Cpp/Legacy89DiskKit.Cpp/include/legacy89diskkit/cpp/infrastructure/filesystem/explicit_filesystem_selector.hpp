#pragma once

#include "legacy89diskkit/cpp/filesystem_surface_catalog.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/hu_basic/hu_basic_file_system.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/msx_dos/explicit_msx_dos_file_system.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/n88_basic/n88_basic_file_system.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string_view>
#include <variant>

namespace legacy89diskkit::cpp
{
using ExplicitFileSystemSelection = std::variant<std::monostate, HuBasicFileSystem, N88BasicFileSystem, MsxDosFileSystem>;

class ExplicitFileSystemSelector
{
public:
    static Result<FileSystemFamily> ParseFamily(std::string_view file_system_name);
    static std::string_view GetCanonicalName(FileSystemFamily family);
    static bool SupportsDiskType(FileSystemFamily family, DiskType disk_type);
    static FileSystemFamily GetFamily(const ExplicitFileSystemSelection& selection);

    static Result<ExplicitFileSystemSelection> Open(std::string_view file_system_name, RawDiskContainer& container);
    static Result<ExplicitFileSystemSelection> Open(std::string_view file_system_name, D88DiskContainer& container);
};
}
