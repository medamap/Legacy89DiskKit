#include "legacy89diskkit/cpp/application/drive_mount_service.hpp"
#include <stdexcept>

namespace legacy89diskkit::cpp::application
{
void DriveMountService::Mount(int drive_number, std::shared_ptr<MountedMedium> medium)
{
    if (!medium)
    {
        throw std::invalid_argument("medium cannot be null");
    }

    if (drive_number < 0)
    {
        throw std::out_of_range("drive number must be zero or greater");
    }

    mounted_media_[drive_number] = std::move(medium);
}

bool DriveMountService::Unmount(int drive_number)
{
    return mounted_media_.erase(drive_number) > 0;
}

bool DriveMountService::IsMounted(int drive_number) const
{
    return mounted_media_.find(drive_number) != mounted_media_.end();
}

std::shared_ptr<MountedMedium> DriveMountService::GetMountedMedium(int drive_number) const
{
    auto it = mounted_media_.find(drive_number);
    return (it != mounted_media_.end()) ? it->second : nullptr;
}

void DriveMountService::SetMotorOn(int drive_number, bool on)
{
    motor_states_[drive_number] = on;
}

bool DriveMountService::IsMotorOn(int drive_number) const
{
    auto it = motor_states_.find(drive_number);
    return (it != motor_states_.end()) ? it->second : false;
}

DriveState DriveMountService::GetState(
    int drive_number,
    int current_track,
    int selected_side,
    bool is_write_protected) const
{
    auto medium = GetMountedMedium(drive_number);
    bool has_medium = medium != nullptr;
    bool motor_on = IsMotorOn(drive_number);

    return DriveState{
        drive_number,
        has_medium,
        current_track,
        selected_side,
        motor_on,
        has_medium && motor_on, // is_ready requires medium + motor ON
        is_write_protected,
        has_medium ? medium->MediumKind() : ""
    };
}
} // namespace legacy89diskkit::cpp::application
