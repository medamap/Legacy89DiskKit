#pragma once

#include "legacy89diskkit/cpp/infrastructure/filesystem/msx_dos/msx_dos_file_system.hpp"

namespace legacy89diskkit::cpp
{
class ExplicitMsxDosFileSystem
{
public:
    static MsxDosFileSystem Open(RawDiskContainer& container);
    static MsxDosFileSystem Open(D88DiskContainer& container);
};
}
