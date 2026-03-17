#pragma once

#include "legacy89diskkit/cpp/application/drive_mount_service.hpp"
#include "legacy89diskkit/cpp/application/mounted_medium_binding_service.hpp"
#include "legacy89diskkit/cpp/application/emulator_host_types.hpp"
#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include <functional>
#include <map>
#include <memory>

namespace legacy89diskkit::cpp::application
{
class EventDrivenEmulatorFdcHostAdapter
{
public:
    EventDrivenEmulatorFdcHostAdapter(
        std::shared_ptr<DriveMountService> drive_mount_service,
        std::shared_ptr<MountedMediumBindingService> binding_service);

    // Callbacks
    std::function<void(bool)> on_irq_changed;
    std::function<void(bool)> on_drq_changed;
    std::function<void(std::chrono::nanoseconds)> on_advance_requested;

    // Command Handlers
    EmulatorHostResponse Handle(const EmulatorHostRequest& request);

    // High-level operations
    void OpenDisk(int drive_number, NativeFileSystemSession& session);
    bool CloseDisk(int drive_number);
    bool IsDiskInserted(int drive_number) const;
    bool IsDriveReady(int drive_number) const;
    void SetMotorOn(int drive_number, bool on);
    void SelectDrive(int drive_number);
    void SelectSide(int side);
    void Reset();
    void WriteIo8(std::uint32_t address, std::uint8_t value);
    std::uint8_t ReadIo8(std::uint32_t address);
    void Advance(std::chrono::nanoseconds delta);

private:
    struct ControllerBinding
    {
        std::shared_ptr<FdcController> controller;
        std::shared_ptr<TimedFdcController> timed_controller;
    };

    void SyncSignals();
    std::optional<FdcVisibleState> TryGetVisibleState() const;
    std::optional<std::chrono::nanoseconds> TryGetPendingAdvanceHint() const;
    const ControllerBinding& GetCurrentController() const;

    std::shared_ptr<DriveMountService> drive_mount_service_;
    std::shared_ptr<MountedMediumBindingService> binding_service_;

    std::map<int, MountedMediumBinding> bindings_;
    std::map<int, ControllerBinding> controllers_;
    int selected_drive_ = 0;

    bool last_irq_ = false;
    bool last_drq_ = false;
    std::optional<std::chrono::nanoseconds> last_advance_hint_;

    static FdcRegister MapRegister(std::uint32_t address);
};
} // namespace legacy89diskkit::cpp::application
