#include "legacy89diskkit/cpp/infrastructure/filesystem/msx_dos/explicit_msx_dos_file_system.hpp"

#include "legacy89diskkit/cpp/msx_dos_configuration.hpp"

namespace legacy89diskkit::cpp
{
MsxDosFileSystem ExplicitMsxDosFileSystem::Open(RawDiskContainer& container)
{
    return MsxDosFileSystem::OpenExplicit(container);
}

MsxDosFileSystem ExplicitMsxDosFileSystem::Open(D88DiskContainer& container)
{
    return MsxDosFileSystem::OpenExplicit(container);
}
}
