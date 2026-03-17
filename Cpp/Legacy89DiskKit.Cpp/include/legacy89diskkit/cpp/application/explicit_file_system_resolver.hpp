#pragma once

#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string_view>

namespace legacy89diskkit::cpp::application
{
class ExplicitFileSystemResolver
{
public:
    ExplicitFileSystemResolver() = default;

    /**
     * Initializes the disk session for detection.
     * For Hu-BASIC, this writes the magic byte to the boot sector
     * so that subsequent automatic detection can identify it.
     */
    Status InitializeForDetection(NativeFileSystemSession& session) const;

    /**
     * Returns the canonical name for a given file system family.
     */
    static std::string_view GetCanonicalName(FileSystemFamily family);
};
} // namespace legacy89diskkit::cpp::application
