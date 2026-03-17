#include "legacy89diskkit/cpp/application/event_driven_emulator_fdc_host_adapter.hpp"
#include <stdexcept>
#include <algorithm>

namespace legacy89diskkit::cpp::application
{
namespace
{
const EmulatorHostCapabilities HostCapabilities = {
    1,     // protocol_version
    true,  // supports_path_open
    true,  // supports_buffer_open
    true,  // supports_notification_exchange
    true,  // supports_plain_stdio
    true   // supports_observable_stdio
};
}

EventDrivenEmulatorFdcHostAdapter::EventDrivenEmulatorFdcHostAdapter(
    std::shared_ptr<DriveMountService> drive_mount_service,
    std::shared_ptr<MountedMediumBindingService> binding_service)
    : drive_mount_service_(std::move(drive_mount_service)),
      binding_service_(std::move(binding_service))
{
}

void EventDrivenEmulatorFdcHostAdapter::OpenDisk(int drive_number, NativeFileSystemSession& session)
{
    auto binding_result = binding_service_->MountSession(*drive_mount_service_, drive_number, session);
    if (!binding_result.ok())
    {
        throw std::runtime_error(binding_result.status().message);
    }

    auto binding = binding_result.value();
    auto controller_result = binding_service_->CreateController(binding, drive_number);
    if (!controller_result.ok())
    {
        throw std::runtime_error(controller_result.status().message);
    }

    auto controller = controller_result.value();
    auto timed_controller = std::dynamic_pointer_cast<TimedFdcController>(controller);
    if (!timed_controller)
    {
        throw std::runtime_error("The mounted medium controller does not support timing advancement.");
    }

    bindings_[drive_number] = std::move(binding);
    controllers_[drive_number] = ControllerBinding{std::move(controller), std::move(timed_controller)};
    SyncSignals();
}

bool EventDrivenEmulatorFdcHostAdapter::CloseDisk(int drive_number)
{
    bindings_.erase(drive_number);
    controllers_.erase(drive_number);
    bool unmounted = drive_mount_service_->Unmount(drive_number);
    SyncSignals();
    return unmounted;
}

bool EventDrivenEmulatorFdcHostAdapter::IsDiskInserted(int drive_number) const
{
    return drive_mount_service_->IsMounted(drive_number);
}

bool EventDrivenEmulatorFdcHostAdapter::IsDriveReady(int drive_number) const
{
    return drive_mount_service_->GetState(drive_number).is_ready;
}

void EventDrivenEmulatorFdcHostAdapter::SetMotorOn(int drive_number, bool on)
{
    drive_mount_service_->SetMotorOn(drive_number, on);
    SyncSignals();
}

void EventDrivenEmulatorFdcHostAdapter::SelectDrive(int drive_number)
{
    selected_drive_ = drive_number;
    SyncSignals();
}

void EventDrivenEmulatorFdcHostAdapter::SelectSide(int side)
{
    auto it = bindings_.find(selected_drive_);
    if (it == bindings_.end() || !it->second.controller_facing_medium)
    {
        throw std::runtime_error("No controller-facing medium is mounted for the selected drive.");
    }

    it->second.controller_facing_medium->SelectSide(side);
    SyncSignals();
}

void EventDrivenEmulatorFdcHostAdapter::Reset()
{
    GetCurrentController().controller->Reset();
    SyncSignals();
}

void EventDrivenEmulatorFdcHostAdapter::WriteIo8(std::uint32_t address, std::uint8_t value)
{
    GetCurrentController().controller->WriteRegister(MapRegister(address), value);
    SyncSignals();
}

std::uint8_t EventDrivenEmulatorFdcHostAdapter::ReadIo8(std::uint32_t address)
{
    std::uint8_t value = GetCurrentController().controller->ReadRegister(MapRegister(address));
    SyncSignals();
    return value;
}

void EventDrivenEmulatorFdcHostAdapter::Advance(std::chrono::nanoseconds delta)
{
    GetCurrentController().timed_controller->Advance(delta);
    SyncSignals();
}

EmulatorHostResponse EventDrivenEmulatorFdcHostAdapter::Handle(const EmulatorHostRequest& request)
{
    std::optional<std::uint8_t> register_value = std::nullopt;
    std::optional<std::string> error_message = std::nullopt;

    try
    {
        switch (request.kind)
        {
        case EmulatorHostRequestKind::QueryCapabilities:
            break;
        case EmulatorHostRequestKind::OpenDiskPath:
            error_message = "OpenDiskPath is not yet implemented in the C++ Application layer.";
            break;
        case EmulatorHostRequestKind::OpenDiskImage:
            error_message = "OpenDiskImage is not yet implemented in the C++ Application layer.";
            break;
        case EmulatorHostRequestKind::CloseDisk:
            CloseDisk(request.drive_number.value_or(0));
            break;
        case EmulatorHostRequestKind::SelectDrive:
            SelectDrive(request.drive_number.value_or(0));
            break;
        case EmulatorHostRequestKind::SelectSide:
            SelectSide(request.side.value_or(0));
            break;
        case EmulatorHostRequestKind::Reset:
            Reset();
            break;
        case EmulatorHostRequestKind::WriteRegister:
            WriteIo8(request.register_address.value_or(0), request.register_value.value_or(0));
            break;
        case EmulatorHostRequestKind::ReadRegister:
            register_value = ReadIo8(request.register_address.value_or(0));
            break;
        case EmulatorHostRequestKind::Advance:
            Advance(std::chrono::microseconds(request.advance_microseconds.value_or(0)));
            break;
        case EmulatorHostRequestKind::QueryState:
            break;
        }
    }
    catch (const std::exception& e)
    {
        error_message = e.what();
    }

    auto visible_state = TryGetVisibleState();
    auto pending_advance = TryGetPendingAdvanceHint();

    EmulatorHostResponse response;
    response.register_value = register_value;
    response.visible_state = visible_state;
    response.irq = visible_state ? visible_state->irq : false;
    response.drq = visible_state ? visible_state->drq : false;
    response.pending_advance_microseconds = pending_advance ? std::optional<std::int64_t>(std::chrono::duration_cast<std::chrono::microseconds>(*pending_advance).count()) : std::nullopt;
    response.error_message = error_message;
    
    if (request.kind == EmulatorHostRequestKind::QueryCapabilities)
    {
        response.capabilities = HostCapabilities;
    }

    return response;
}

void EventDrivenEmulatorFdcHostAdapter::SyncSignals()
{
    auto visible = TryGetVisibleState();
    bool irq = visible ? visible->irq : false;
    bool drq = visible ? visible->drq : false;
    auto hint = TryGetPendingAdvanceHint();

    if (irq != last_irq_)
    {
        last_irq_ = irq;
        if (on_irq_changed) on_irq_changed(irq);
    }

    if (drq != last_drq_)
    {
        last_drq_ = drq;
        if (on_drq_changed) on_drq_changed(drq);
    }

    if (hint.has_value())
    {
        if (!last_advance_hint_.has_value() || *hint != *last_advance_hint_)
        {
            last_advance_hint_ = hint;
            if (on_advance_requested) on_advance_requested(*hint);
        }
    }
    else
    {
        last_advance_hint_ = std::nullopt;
    }
}

std::optional<FdcVisibleState> EventDrivenEmulatorFdcHostAdapter::TryGetVisibleState() const
{
    auto it = controllers_.find(selected_drive_);
    if (it != controllers_.end())
    {
        return it->second.controller->GetVisibleState();
    }
    return std::nullopt;
}

std::optional<std::chrono::nanoseconds> EventDrivenEmulatorFdcHostAdapter::TryGetPendingAdvanceHint() const
{
    auto it = controllers_.find(selected_drive_);
    if (it != controllers_.end())
    {
        return it->second.timed_controller->GetPendingAdvanceHint();
    }
    return std::nullopt;
}

const EventDrivenEmulatorFdcHostAdapter::ControllerBinding& EventDrivenEmulatorFdcHostAdapter::GetCurrentController() const
{
    auto it = controllers_.find(selected_drive_);
    if (it == controllers_.end())
    {
        throw std::runtime_error("No disk is mounted for the selected drive.");
    }
    return it->second;
}

FdcRegister EventDrivenEmulatorFdcHostAdapter::MapRegister(std::uint32_t address)
{
    switch (address)
    {
    case 0: return FdcRegister::CommandStatus;
    case 1: return FdcRegister::Track;
    case 2: return FdcRegister::Sector;
    case 3: return FdcRegister::Data;
    default: throw std::out_of_range("Unsupported FDC register address.");
    }
}
} // namespace legacy89diskkit::cpp::application
