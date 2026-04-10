#pragma once

#include "legacy89diskkit/cpp/domain/fdc/fdc_types.hpp"

#include <chrono>
#include <optional>

namespace legacy89diskkit::cpp
{
class FdcController
{
public:
    virtual ~FdcController() = default;

    virtual void Reset() = 0;
    virtual void WriteRegister(FdcRegister reg, std::uint8_t value) = 0;
    virtual std::uint8_t ReadRegister(FdcRegister reg) = 0;
    virtual FdcVisibleState GetVisibleState() const = 0;
};

class TimedFdcController
{
public:
    virtual ~TimedFdcController() = default;

    virtual std::optional<std::chrono::nanoseconds> GetPendingAdvanceHint() const = 0;
    virtual void Advance(std::chrono::nanoseconds delta) = 0;
};
}
