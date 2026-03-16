#include "legacy89diskkit/cpp/domain/controller/controller_runtime_contracts.hpp"
#include "legacy89diskkit/cpp/domain/drive/sector_addressable_medium_contracts.hpp"
#include "legacy89diskkit/cpp/domain/fdc/fdc_controller_contracts.hpp"

#include <chrono>
#include <optional>
#include <string>

using namespace legacy89diskkit::cpp;

namespace
{
class StubMedium final : public ControllerFacingMedium
{
public:
    const std::string& MediumKind() const override
    {
        return medium_kind_;
    }

    bool IsReady() const override
    {
        return true;
    }

    bool IsWriteProtected() const override
    {
        return false;
    }

    int SelectedSide() const override
    {
        return 0;
    }

    bool IsBusy() const override
    {
        return false;
    }

    std::optional<std::chrono::nanoseconds> GetPendingDelayHint() const override
    {
        return std::chrono::microseconds(10);
    }

    void Reset() override {}
    void Advance(std::chrono::nanoseconds) override {}
    void SelectSide(int) override {}
    void SeekTrack(int) override {}

    std::uint8_t ReadStatus() override
    {
        return FdcStatusBusy;
    }

    std::uint8_t ReadTrackRegister() const override
    {
        return 1;
    }

    std::uint8_t ReadSectorRegister() const override
    {
        return 2;
    }

    std::uint8_t PeekDataRegister() const override
    {
        return 3;
    }

    std::uint8_t ReadDataRegister() override
    {
        return 4;
    }

    void WriteCommand(std::uint8_t) override {}
    void WriteTrackRegister(std::uint8_t) override {}
    void WriteSectorRegister(std::uint8_t) override {}
    void WriteDataRegister(std::uint8_t) override {}

    bool IsIrqAsserted() const override
    {
        return true;
    }

    bool IsDrqAsserted() const override
    {
        return false;
    }

private:
    std::string medium_kind_{"d88"};
};

class StubClock final : public ControllerClock
{
public:
    std::chrono::nanoseconds Elapsed() const override
    {
        return elapsed_;
    }

    void Advance(std::chrono::nanoseconds delta) override
    {
        elapsed_ += delta;
    }

private:
    std::chrono::nanoseconds elapsed_{};
};

class StubDrive final : public FloppyDrive
{
public:
    int DriveNumber() const override
    {
        return 0;
    }

    DriveState GetState() const override
    {
        return {0, true, 12, 1, true, true, false, std::string("d88")};
    }
};

class StubSectorMedium final : public SectorAddressableMedium
{
public:
    const std::string& MediumKind() const override
    {
        return medium_kind_;
    }

    bool SupportsDirectImageAccess() const override
    {
        return true;
    }

    bool SupportsControllerFacingAccess() const override
    {
        return true;
    }

    bool SectorExists(int cylinder, int head, int sector) const override
    {
        return cylinder == 0 && head == 0 && sector == 1;
    }

    std::vector<std::uint8_t> ReadSector(int cylinder, int head, int sector, bool) const override
    {
        if (!SectorExists(cylinder, head, sector))
        {
            return {};
        }

        return {0x12, 0x34, 0x56};
    }

    std::vector<SectorInfo> GetAllSectors() const override
    {
        return {{0, 0, 1, 256, false, false}};
    }

private:
    std::string medium_kind_{"d88"};
};

class StubController final : public FdcController, public TimedFdcController
{
public:
    void Reset() override
    {
        state_ = {0, 0, 0, 0, 0, 0, false, false, false};
    }

    void WriteRegister(FdcRegister reg, std::uint8_t value) override
    {
        switch (reg)
        {
        case FdcRegister::CommandStatus:
            state_.status = value;
            break;
        case FdcRegister::Track:
            state_.track = value;
            break;
        case FdcRegister::Sector:
            state_.sector = value;
            break;
        case FdcRegister::Data:
            state_.data = value;
            break;
        }
    }

    std::uint8_t ReadRegister(FdcRegister reg) override
    {
        switch (reg)
        {
        case FdcRegister::CommandStatus:
            return state_.status;
        case FdcRegister::Track:
            return state_.track;
        case FdcRegister::Sector:
            return state_.sector;
        case FdcRegister::Data:
            return state_.data;
        }

        return 0;
    }

    FdcVisibleState GetVisibleState() const override
    {
        return state_;
    }

    std::optional<std::chrono::nanoseconds> GetPendingAdvanceHint() const override
    {
        return std::chrono::microseconds(5);
    }

    void Advance(std::chrono::nanoseconds delta) override
    {
        elapsed_ += delta;
    }

    std::chrono::nanoseconds elapsed_{};
    FdcVisibleState state_{0, 0, 0, 0, 0, 0, false, false, false};
};
}

int main()
{
    StubMedium medium;
    StubClock clock;
    StubDrive drive;
    StubSectorMedium sector_medium;
    StubController controller;

    controller.WriteRegister(FdcRegister::Track, 7);
    controller.WriteRegister(FdcRegister::Sector, 9);
    clock.Advance(std::chrono::microseconds(20));
    controller.Advance(std::chrono::microseconds(5));

    const auto state = controller.GetVisibleState();
    const auto drive_state = drive.GetState();

    if (medium.MediumKind() != "d88")
    {
        return 1;
    }

    if (medium.ReadStatus() != FdcStatusBusy)
    {
        return 2;
    }

    if (state.track != 7 || state.sector != 9)
    {
        return 3;
    }

    if (clock.Elapsed() != std::chrono::microseconds(20))
    {
        return 4;
    }

    if (drive_state.current_track != 12 || drive_state.selected_side != 1)
    {
        return 5;
    }

    if (!controller.GetPendingAdvanceHint().has_value() || !medium.GetPendingDelayHint().has_value())
    {
        return 6;
    }

    if (!sector_medium.SupportsDirectImageAccess() || !sector_medium.SupportsControllerFacingAccess())
    {
        return 7;
    }

    if (!sector_medium.SectorExists(0, 0, 1) || sector_medium.SectorExists(0, 0, 2))
    {
        return 8;
    }

    const auto data = sector_medium.ReadSector(0, 0, 1, false);
    if (data.size() != 3 || data[0] != 0x12)
    {
        return 9;
    }

    const auto sectors = sector_medium.GetAllSectors();
    if (sectors.size() != 1 || sectors[0].sector != 1)
    {
        return 10;
    }

    return 0;
}
