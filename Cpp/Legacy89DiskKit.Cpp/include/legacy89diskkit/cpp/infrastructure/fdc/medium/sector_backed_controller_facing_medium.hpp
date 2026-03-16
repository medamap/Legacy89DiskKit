#pragma once

#include "legacy89diskkit/cpp/domain/controller/controller_runtime_contracts.hpp"
#include "legacy89diskkit/cpp/domain/fdc/fdc_types.hpp"

#include <chrono>
#include <vector>

namespace legacy89diskkit::cpp
{
class SectorBackedControllerFacingMedium : public ControllerFacingMedium
{
public:
    bool IsReady() const override;
    int SelectedSide() const override;
    bool IsBusy() const override;
    std::optional<std::chrono::nanoseconds> GetPendingDelayHint() const override;
    bool IsIrqAsserted() const override;
    bool IsDrqAsserted() const override;

    void Reset() override;
    void Advance(std::chrono::nanoseconds delta) override;
    void SelectSide(int side) override;
    void SeekTrack(int track) override;

    std::uint8_t ReadStatus() override;
    std::uint8_t ReadTrackRegister() const override;
    std::uint8_t ReadSectorRegister() const override;
    std::uint8_t PeekDataRegister() const override;
    std::uint8_t ReadDataRegister() override;

    void WriteCommand(std::uint8_t value) override;
    void WriteTrackRegister(std::uint8_t value) override;
    void WriteSectorRegister(std::uint8_t value) override;
    void WriteDataRegister(std::uint8_t value) override;

protected:
    virtual bool SectorExistsCore(int track, int side, int sector) const = 0;
    virtual std::vector<std::uint8_t> ReadSectorCore(int track, int side, int sector) const = 0;

private:
    enum class PendingOperation
    {
        None,
        Restore,
        Seek,
        ReadSector
    };

    void StartPendingOperation(PendingOperation operation);
    void CompletePendingOperation();
    void ExecuteForceInterrupt();
    void CompleteReadSector();
    void BeginTransfer(std::vector<std::uint8_t> data);
    void AdvanceTransfer();
    void ClearTransfer();

    static constexpr auto kCommandDelay = std::chrono::milliseconds(1);

    std::uint8_t status_{0};
    std::uint8_t track_{0};
    std::uint8_t sector_{1};
    std::uint8_t data_{0};
    int selected_side_{0};
    bool irq_{false};
    bool drq_{false};
    std::vector<std::uint8_t> transfer_buffer_;
    std::size_t transfer_index_{0};
    PendingOperation pending_operation_{PendingOperation::None};
    std::chrono::nanoseconds remaining_delay_{};
    std::uint8_t pending_seek_track_{0};
};
}
