#pragma once

#include "legacy89diskkit/cpp/domain/fdc/fdc_controller_contracts.hpp"
#include "legacy89diskkit/cpp/domain/controller/controller_runtime_contracts.hpp"

#include <memory>

namespace legacy89diskkit::cpp
{
class FdcMediumController : public FdcController, public TimedFdcController
{
public:
    explicit FdcMediumController(std::shared_ptr<ControllerFacingMedium> medium, int selected_drive = 0);

    // FdcController
    void Reset() override;
    void WriteRegister(FdcRegister reg, std::uint8_t value) override;
    std::uint8_t ReadRegister(FdcRegister reg) override;
    FdcVisibleState GetVisibleState() const override;

    // TimedFdcController
    std::optional<std::chrono::nanoseconds> GetPendingAdvanceHint() const override;
    void Advance(std::chrono::nanoseconds delta) override;

private:
    std::shared_ptr<ControllerFacingMedium> medium_;
    int selected_drive_;
};
} // namespace legacy89diskkit::cpp
