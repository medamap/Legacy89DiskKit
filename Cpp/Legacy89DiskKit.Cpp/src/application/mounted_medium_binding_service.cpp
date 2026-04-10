#include "legacy89diskkit/cpp/application/mounted_medium_binding_service.hpp"
#include "legacy89diskkit/cpp/infrastructure/drive/mounted_medium_binding_factory.hpp"
#include "legacy89diskkit/cpp/infrastructure/fdc/fdc_medium_controller.hpp"

#include <variant>

namespace legacy89diskkit::cpp::application
{
Result<MountedMediumBinding> MountedMediumBindingService::CreateFromSession(NativeFileSystemSession& session)
{
    return session.ApplyToContainer(
        [](auto& container) -> Result<MountedMediumBinding>
        {
            using T = std::decay_t<decltype(container)>;
            if constexpr (std::is_same_v<T, std::monostate>)
            {
                return Result<MountedMediumBinding>::Failure(StatusCode::InvalidArgument, "Container is not initialized.");
            }
            else
            {
                return Result<MountedMediumBinding>::Success(MountedMediumBindingFactory::Create(container));
            }
        });
}

Result<MountedMediumBinding> MountedMediumBindingService::MountSession(
    DriveMountService& drive_mount_service,
    int drive_number,
    NativeFileSystemSession& session)
{
    auto binding_result = CreateFromSession(session);
    if (!binding_result.ok())
    {
        return binding_result;
    }

    auto binding = binding_result.value();
    drive_mount_service.Mount(drive_number, binding.mounted_medium);
    return Result<MountedMediumBinding>::Success(std::move(binding));
}

Result<std::shared_ptr<FdcController>> MountedMediumBindingService::CreateController(const MountedMediumBinding& binding, int drive_number)
{
    if (!binding.controller_facing_medium)
    {
        return Result<std::shared_ptr<FdcController>>::Failure(StatusCode::UnsupportedFormat, "The mounted medium does not expose a controller-facing adapter.");
    }

    auto controller = std::make_shared<FdcMediumController>(binding.controller_facing_medium, drive_number);
    return Result<std::shared_ptr<FdcController>>::Success(std::move(controller));
}
} // namespace legacy89diskkit::cpp::application
