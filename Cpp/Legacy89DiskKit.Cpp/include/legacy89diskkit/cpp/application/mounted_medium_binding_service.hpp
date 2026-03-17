#pragma once

#include "legacy89diskkit/cpp/application/drive_mount_service.hpp"
#include "legacy89diskkit/cpp/infrastructure/drive/mounted_medium_binding.hpp"
#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"
#include "legacy89diskkit/cpp/domain/fdc/fdc_controller_contracts.hpp"

#include <memory>

namespace legacy89diskkit::cpp::application
{
class MountedMediumBindingService
{
public:
    MountedMediumBindingService() = default;

    /**
     * Creates a binding from a native session's underlying container.
     */
    Result<MountedMediumBinding> CreateFromSession(NativeFileSystemSession& session);

    /**
     * Mounts a session's container to a drive and returns the binding.
     */
    Result<MountedMediumBinding> MountSession(
        DriveMountService& drive_mount_service,
        int drive_number,
        NativeFileSystemSession& session);

    /**
     * Creates an FDC controller from a binding.
     */
    Result<std::shared_ptr<FdcController>> CreateController(const MountedMediumBinding& binding, int drive_number);
};
} // namespace legacy89diskkit::cpp::application
