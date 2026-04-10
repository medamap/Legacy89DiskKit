#include "legacy89diskkit/cpp/infrastructure/fdc/fdc_medium_controller.hpp"
#include <stdexcept>

namespace legacy89diskkit::cpp
{
FdcMediumController::FdcMediumController(std::shared_ptr<ControllerFacingMedium> medium, int selected_drive)
    : medium_(std::move(medium)), selected_drive_(selected_drive)
{
    if (!medium_)
    {
        throw std::invalid_argument("medium cannot be null");
    }
}

void FdcMediumController::Reset()
{
    medium_->Reset();
}

void FdcMediumController::WriteRegister(FdcRegister reg, std::uint8_t value)
{
    switch (reg)
    {
    case FdcRegister::CommandStatus:
        medium_->WriteCommand(value);
        break;
    case FdcRegister::Track:
        medium_->WriteTrackRegister(value);
        break;
    case FdcRegister::Sector:
        medium_->WriteSectorRegister(value);
        break;
    case FdcRegister::Data:
        medium_->WriteDataRegister(value);
        break;
    default:
        // C++ style: ignore or log unsupported registers instead of throwing in high-frequency path
        break;
    }
}

std::uint8_t FdcMediumController::ReadRegister(FdcRegister reg)
{
    switch (reg)
    {
    case FdcRegister::CommandStatus:
        return medium_->ReadStatus();
    case FdcRegister::Track:
        return medium_->ReadTrackRegister();
    case FdcRegister::Sector:
        return medium_->ReadSectorRegister();
    case FdcRegister::Data:
        return medium_->ReadDataRegister();
    default:
        return 0xFF;
    }
}

FdcVisibleState FdcMediumController::GetVisibleState() const
{
    return FdcVisibleState{
        medium_->ReadStatus(),
        medium_->ReadTrackRegister(),
        medium_->ReadSectorRegister(),
        medium_->PeekDataRegister(),
        selected_drive_,
        medium_->SelectedSide(),
        medium_->IsBusy(),
        medium_->IsIrqAsserted(),
        medium_->IsDrqAsserted()
    };
}

std::optional<std::chrono::nanoseconds> FdcMediumController::GetPendingAdvanceHint() const
{
    return medium_->GetPendingDelayHint();
}

void FdcMediumController::Advance(std::chrono::nanoseconds delta)
{
    medium_->Advance(delta);
}
} // namespace legacy89diskkit::cpp
