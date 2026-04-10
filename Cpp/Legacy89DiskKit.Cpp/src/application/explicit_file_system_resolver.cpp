#include "legacy89diskkit/cpp/application/explicit_file_system_resolver.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/explicit_filesystem_selector.hpp"

#include <vector>

namespace legacy89diskkit::cpp::application
{
Status ExplicitFileSystemResolver::InitializeForDetection(NativeFileSystemSession& session) const
{
    if (session.Family() == FileSystemFamily::HuBasic)
    {
        // Hu-BASIC needs a magic byte (0x01) at the beginning of the boot sector (C0/H0/S1)
        // and "Sys" at offset 0x0e so that the scoring-based detection can identify it reliably.
        auto read_result = session.ReadBootArea();
        if (!read_result.ok())
        {
            return read_result.status();
        }

        auto boot_data = std::move(read_result.value());
        if (boot_data.size() < 256)
        {
            boot_data.resize(256, 0x00); // Default sector size
        }

        bool modified = false;
        if (boot_data[0] != 0x01)
        {
            boot_data[0] = 0x01;
            modified = true;
        }

        if (boot_data[0x0e] != 'S' || boot_data[0x0f] != 'y' || boot_data[0x10] != 's')
        {
            boot_data[0x0e] = 'S';
            boot_data[0x0f] = 'y';
            boot_data[0x10] = 's';
            modified = true;
        }

        if (modified)
        {
            return session.WriteBootArea(boot_data);
        }
    }

    // N88-BASIC and MSX-DOS typically have their signatures written during Format(),
    // so no additional initialization is required here for now.
    return Status::OkStatus();
}

std::string_view ExplicitFileSystemResolver::GetCanonicalName(FileSystemFamily family)
{
    return ExplicitFileSystemSelector::GetCanonicalName(family);
}
} // namespace legacy89diskkit::cpp::application
