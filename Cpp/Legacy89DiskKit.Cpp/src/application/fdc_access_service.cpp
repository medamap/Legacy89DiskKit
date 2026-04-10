#include "legacy89diskkit/cpp/application/fdc_access_service.hpp"
#include <stdexcept>

namespace legacy89diskkit::cpp::application
{
FdcAccessService::FdcAccessService(
    std::shared_ptr<FdcController> controller,
    std::shared_ptr<ControllerClock> clock)
    : controller_(std::move(controller)), clock_(std::move(clock))
{
    if (!controller_)
    {
        throw std::invalid_argument("controller cannot be null");
    }
}

bool FdcAccessService::SupportsTimingAdvance() const
{
    return clock_ != nullptr;
}

void FdcAccessService::Reset()
{
    controller_->Reset();
}

void FdcAccessService::WriteRegister(FdcRegister reg, std::uint8_t value)
{
    controller_->WriteRegister(reg, value);
}

std::uint8_t FdcAccessService::ReadRegister(FdcRegister reg)
{
    return controller_->ReadRegister(reg);
}

FdcVisibleState FdcAccessService::GetVisibleState() const
{
    return controller_->GetVisibleState();
}

void FdcAccessService::Advance(std::chrono::nanoseconds delta)
{
    if (!clock_)
    {
        throw std::logic_error("Timing advance is not available without a controller clock.");
    }

    // Update the master clock for the controller environment
    clock_->Advance(delta);

    // Notify the controller to update its internal state based on the same delta.
    // Note: If the controller and clock share the same internal state object, 
    // it is up to the implementation to avoid double-advancement.
    // In our default wiring, they are separate observers of the timeline.
    auto timed_controller = std::dynamic_pointer_cast<TimedFdcController>(controller_);
    if (timed_controller)
    {
        timed_controller->Advance(delta);
    }
}
} // namespace legacy89diskkit::cpp::application
