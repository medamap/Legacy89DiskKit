#pragma once

#include "legacy89diskkit/cpp/domain/drive/mounted_medium_contracts.hpp"
#include "legacy89diskkit/cpp/domain/drive/drive_types.hpp"

#include <map>
#include <memory>

namespace legacy89diskkit::cpp::application
{
class DriveMountService
{
public:
    DriveMountService() = default;

    void Mount(int drive_number, std::shared_ptr<MountedMedium> medium);
    bool Unmount(int drive_number);
    bool IsMounted(int drive_number) const;
    std::shared_ptr<MountedMedium> GetMountedMedium(int drive_number) const;

    void SetMotorOn(int drive_number, bool on);
    bool IsMotorOn(int drive_number) const;

    DriveState GetState(
        int drive_number,
        int current_track = 0,
        int selected_side = 0,
        bool is_write_protected = false) const;

private:
    std::map<int, std::shared_ptr<MountedMedium>> mounted_media_;
    std::map<int, bool> motor_states_;
};
} // namespace legacy89diskkit::cpp::application
