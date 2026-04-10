#include "legacy89diskkit/cpp/application/legacy89diskkit_application.hpp"

namespace legacy89diskkit::cpp::application
{
DiskService CreateDiskService()
{
    return DiskService();
}

FileTransferService CreateFileTransferService(NativeFileSystemSession* session)
{
    return FileTransferService(session);
}

DirectoryLayoutService CreateDirectoryLayoutService(NativeFileSystemSession* session)
{
    return DirectoryLayoutService(session);
}

BootAndCloneService CreateBootAndCloneService()
{
    return BootAndCloneService();
}

ExplicitFileSystemResolver CreateExplicitFileSystemResolver()
{
    return ExplicitFileSystemResolver();
}
} // namespace legacy89diskkit::cpp::application
