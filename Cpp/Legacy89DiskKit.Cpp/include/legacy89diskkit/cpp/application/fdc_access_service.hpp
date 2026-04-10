#pragma once

#include "legacy89diskkit/cpp/domain/fdc/fdc_controller_contracts.hpp"
#include "legacy89diskkit/cpp/domain/controller/controller_runtime_contracts.hpp"

#include <memory>
#include <chrono>

namespace legacy89diskkit::cpp::application
{
class FdcAccessService
{
public:
    FdcAccessService(
        std::shared_ptr<FdcController> controller,
        std::shared_ptr<ControllerClock> clock = nullptr);

    bool SupportsTimingAdvance() const;
    void Reset();
    void WriteRegister(FdcRegister reg, std::uint8_t value);
    std::uint8_t ReadRegister(FdcRegister reg);
    FdcVisibleState GetVisibleState() const;
    void Advance(std::chrono::nanoseconds delta);

private:
    std::shared_ptr<FdcController> controller_;
    std::shared_ptr<ControllerClock> clock_;
};
} // namespace legacy89diskkit::cpp::application
