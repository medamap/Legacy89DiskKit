#pragma once

#include <cstdint>

namespace legacy89diskkit::cpp
{
enum class FdcRegister : std::uint8_t
{
    CommandStatus = 0,
    Track = 1,
    Sector = 2,
    Data = 3
};

enum FdcStatusFlags : std::uint8_t
{
    FdcStatusNone = 0x00,
    FdcStatusBusy = 0x01,
    FdcStatusRecordNotFound = 0x10,
    FdcStatusUnsupportedCommand = 0x40
};

struct FdcVisibleState
{
    std::uint8_t status;
    std::uint8_t track;
    std::uint8_t sector;
    std::uint8_t data;
    int selected_drive;
    int selected_side;
    bool busy;
    bool irq;
    bool drq;
};
}
