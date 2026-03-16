#pragma once

#include "legacy89diskkit/cpp/domain/drive/drive_types.hpp"

#include <chrono>
#include <cstdint>
#include <optional>
#include <string>

namespace legacy89diskkit::cpp
{
class ControllerFacingMedium
{
public:
    virtual ~ControllerFacingMedium() = default;

    virtual const std::string& MediumKind() const = 0;
    virtual bool IsReady() const = 0;
    virtual bool IsWriteProtected() const = 0;
    virtual int SelectedSide() const = 0;
    virtual bool IsBusy() const = 0;
    virtual std::optional<std::chrono::nanoseconds> GetPendingDelayHint() const = 0;

    virtual void Reset() = 0;
    virtual void Advance(std::chrono::nanoseconds delta) = 0;
    virtual void SelectSide(int side) = 0;
    virtual void SeekTrack(int track) = 0;

    virtual std::uint8_t ReadStatus() = 0;
    virtual std::uint8_t ReadTrackRegister() const = 0;
    virtual std::uint8_t ReadSectorRegister() const = 0;
    virtual std::uint8_t PeekDataRegister() const = 0;
    virtual std::uint8_t ReadDataRegister() = 0;

    virtual void WriteCommand(std::uint8_t value) = 0;
    virtual void WriteTrackRegister(std::uint8_t value) = 0;
    virtual void WriteSectorRegister(std::uint8_t value) = 0;
    virtual void WriteDataRegister(std::uint8_t value) = 0;

    virtual bool IsIrqAsserted() const = 0;
    virtual bool IsDrqAsserted() const = 0;
};

class ControllerClock
{
public:
    virtual ~ControllerClock() = default;

    virtual std::chrono::nanoseconds Elapsed() const = 0;
    virtual void Advance(std::chrono::nanoseconds delta) = 0;
};

class FloppyDrive
{
public:
    virtual ~FloppyDrive() = default;

    virtual int DriveNumber() const = 0;
    virtual DriveState GetState() const = 0;
};
}
