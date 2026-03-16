#pragma once

#include <optional>
#include <string>

namespace legacy89diskkit::cpp
{
struct DriveState
{
    int drive_number;
    bool has_mounted_medium;
    int current_track;
    int selected_side;
    bool motor_on;
    bool is_ready;
    bool is_write_protected;
    std::optional<std::string> mounted_medium_kind;
};
}
