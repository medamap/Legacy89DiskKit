#include "legacy89diskkit/cpp/infrastructure/fdc/medium/sector_backed_controller_facing_medium.hpp"

#include <algorithm>

namespace legacy89diskkit::cpp
{
bool SectorBackedControllerFacingMedium::IsReady() const
{
    return true;
}

int SectorBackedControllerFacingMedium::SelectedSide() const
{
    return selected_side_;
}

bool SectorBackedControllerFacingMedium::IsBusy() const
{
    return pending_operation_ != PendingOperation::None;
}

std::optional<std::chrono::nanoseconds> SectorBackedControllerFacingMedium::GetPendingDelayHint() const
{
    if (pending_operation_ == PendingOperation::None)
    {
        return std::nullopt;
    }

    return remaining_delay_;
}

bool SectorBackedControllerFacingMedium::IsIrqAsserted() const
{
    return irq_;
}

bool SectorBackedControllerFacingMedium::IsDrqAsserted() const
{
    return drq_;
}

void SectorBackedControllerFacingMedium::Reset()
{
    status_ = 0;
    track_ = 0;
    sector_ = 1;
    data_ = 0;
    selected_side_ = 0;
    irq_ = false;
    drq_ = false;
    pending_operation_ = PendingOperation::None;
    remaining_delay_ = std::chrono::nanoseconds::zero();
    pending_seek_track_ = 0;
    ClearTransfer();
}

void SectorBackedControllerFacingMedium::Advance(const std::chrono::nanoseconds delta)
{
    if (pending_operation_ == PendingOperation::None || delta <= std::chrono::nanoseconds::zero())
    {
        return;
    }

    remaining_delay_ -= delta;
    if (remaining_delay_ > std::chrono::nanoseconds::zero())
    {
        return;
    }

    CompletePendingOperation();
}

void SectorBackedControllerFacingMedium::SelectSide(const int side)
{
    selected_side_ = side;
}

void SectorBackedControllerFacingMedium::SeekTrack(const int track)
{
    track_ = static_cast<std::uint8_t>(std::clamp(track, 0, 255));
}

std::uint8_t SectorBackedControllerFacingMedium::ReadStatus()
{
    return status_;
}

std::uint8_t SectorBackedControllerFacingMedium::ReadTrackRegister() const
{
    return track_;
}

std::uint8_t SectorBackedControllerFacingMedium::ReadSectorRegister() const
{
    return sector_;
}

std::uint8_t SectorBackedControllerFacingMedium::PeekDataRegister() const
{
    return data_;
}

std::uint8_t SectorBackedControllerFacingMedium::ReadDataRegister()
{
    const auto value = data_;
    if (drq_)
    {
        AdvanceTransfer();
    }

    return value;
}

void SectorBackedControllerFacingMedium::WriteCommand(const std::uint8_t value)
{
    if ((value & 0xf0) == 0x80)
    {
        StartPendingOperation(PendingOperation::ReadSector);
        return;
    }

    if (value <= 0x0f)
    {
        StartPendingOperation(PendingOperation::Restore);
        return;
    }

    if (value >= 0x10 && value <= 0x1f)
    {
        pending_seek_track_ = data_;
        StartPendingOperation(PendingOperation::Seek);
        return;
    }

    if (value == 0xd0)
    {
        ExecuteForceInterrupt();
        return;
    }

    status_ = FdcStatusUnsupportedCommand;
    irq_ = true;
    drq_ = false;
    ClearTransfer();
}

void SectorBackedControllerFacingMedium::WriteTrackRegister(const std::uint8_t value)
{
    track_ = value;
}

void SectorBackedControllerFacingMedium::WriteSectorRegister(const std::uint8_t value)
{
    sector_ = value;
}

void SectorBackedControllerFacingMedium::WriteDataRegister(const std::uint8_t value)
{
    data_ = value;
}

void SectorBackedControllerFacingMedium::StartPendingOperation(const PendingOperation operation)
{
    pending_operation_ = operation;
    remaining_delay_ = kCommandDelay;
    status_ = FdcStatusBusy;
    irq_ = false;
    drq_ = false;
    ClearTransfer();
}

void SectorBackedControllerFacingMedium::CompletePendingOperation()
{
    const auto operation = pending_operation_;
    pending_operation_ = PendingOperation::None;
    remaining_delay_ = std::chrono::nanoseconds::zero();

    switch (operation)
    {
    case PendingOperation::None:
        return;
    case PendingOperation::Restore:
        track_ = 0;
        status_ = FdcStatusNone;
        irq_ = true;
        drq_ = false;
        ClearTransfer();
        return;
    case PendingOperation::Seek:
        track_ = pending_seek_track_;
        status_ = FdcStatusNone;
        irq_ = true;
        drq_ = false;
        ClearTransfer();
        return;
    case PendingOperation::ReadSector:
        CompleteReadSector();
        return;
    }
}

void SectorBackedControllerFacingMedium::ExecuteForceInterrupt()
{
    status_ = FdcStatusNone;
    irq_ = false;
    drq_ = false;
    pending_operation_ = PendingOperation::None;
    remaining_delay_ = std::chrono::nanoseconds::zero();
    ClearTransfer();
}

void SectorBackedControllerFacingMedium::CompleteReadSector()
{
    if (!SectorExistsCore(track_, selected_side_, sector_))
    {
        status_ = FdcStatusRecordNotFound;
        irq_ = true;
        drq_ = false;
        ClearTransfer();
        return;
    }

    status_ = FdcStatusNone;
    irq_ = true;
    BeginTransfer(ReadSectorCore(track_, selected_side_, sector_));
}

void SectorBackedControllerFacingMedium::BeginTransfer(std::vector<std::uint8_t> data)
{
    transfer_buffer_ = data.empty() ? std::vector<std::uint8_t>{0} : std::move(data);
    transfer_index_ = 0;
    data_ = transfer_buffer_.front();
    drq_ = true;
}

void SectorBackedControllerFacingMedium::AdvanceTransfer()
{
    ++transfer_index_;
    if (transfer_index_ >= transfer_buffer_.size())
    {
        drq_ = false;
        data_ = 0;
        ClearTransfer();
        return;
    }

    data_ = transfer_buffer_[transfer_index_];
    drq_ = true;
}

void SectorBackedControllerFacingMedium::ClearTransfer()
{
    transfer_buffer_.clear();
    transfer_index_ = 0;
}
}
