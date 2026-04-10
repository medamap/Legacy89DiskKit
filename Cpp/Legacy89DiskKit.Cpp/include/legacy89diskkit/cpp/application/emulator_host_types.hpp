#pragma once

#include "legacy89diskkit/cpp/domain/fdc/fdc_types.hpp"
#include <string>
#include <optional>
#include <vector>
#include <cstdint>

namespace legacy89diskkit::cpp::application
{
struct EmulatorHostCapabilities
{
    int protocol_version;
    bool supports_path_open;
    bool supports_buffer_open;
    bool supports_notification_exchange;
    bool supports_plain_stdio;
    bool supports_observable_stdio;
};

enum class EmulatorHostRequestKind
{
    QueryCapabilities,
    OpenDiskPath,
    OpenDiskImage,
    CloseDisk,
    SelectDrive,
    SelectSide,
    Reset,
    WriteRegister,
    ReadRegister,
    Advance,
    QueryState
};

struct EmulatorHostRequest
{
    EmulatorHostRequestKind kind;
    std::optional<int> drive_number;
    std::optional<std::string> image_path;
    std::optional<std::string> image_data_base64;
    std::optional<std::string> image_format;
    std::optional<bool> read_only;
    std::optional<int> side;
    std::optional<std::uint32_t> register_address;
    std::optional<std::uint8_t> register_value;
    std::optional<std::int64_t> advance_microseconds;
};

struct EmulatorHostResponse
{
    std::optional<std::uint8_t> register_value;
    std::optional<FdcVisibleState> visible_state;
    bool irq;
    bool drq;
    std::optional<std::int64_t> pending_advance_microseconds;
    std::optional<EmulatorHostCapabilities> capabilities;
    std::optional<std::string> error_message;
};
} // namespace legacy89diskkit::cpp::application
