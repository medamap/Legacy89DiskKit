#pragma once

#include "legacy89diskkit/cpp/application/disk_service.hpp"
#include "legacy89diskkit/cpp/application/file_transfer_service.hpp"
#include "legacy89diskkit/cpp/application/directory_layout_service.hpp"
#include "legacy89diskkit/cpp/application/boot_and_clone_service.hpp"
#include "legacy89diskkit/cpp/application/explicit_file_system_resolver.hpp"

namespace legacy89diskkit::cpp::application
{
/**
 * Creates a preconfigured disk service.
 */
DiskService CreateDiskService();

/**
 * Creates a preconfigured file transfer service for the given session.
 */
FileTransferService CreateFileTransferService(NativeFileSystemSession* session);

/**
 * Creates a preconfigured directory layout service for the given session.
 */
DirectoryLayoutService CreateDirectoryLayoutService(NativeFileSystemSession* session);

/**
 * Creates a preconfigured boot and clone service.
 */
BootAndCloneService CreateBootAndCloneService();

/**
 * Creates an explicit file system resolver service.
 */
ExplicitFileSystemResolver CreateExplicitFileSystemResolver();

} // namespace legacy89diskkit::cpp::application
