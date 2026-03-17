#pragma once

#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string>
#include <vector>
#include <optional>

namespace legacy89diskkit::cpp::application
{
enum class BootInfoMode
{
    None,
    FileBacked,
    SectorResident
};

struct BootInfoSummary
{
    BootInfoMode mode;
    std::optional<std::string> file_name;
    std::optional<std::uint16_t> load_address;
    std::optional<std::uint16_t> execution_address;
};

class BootAndCloneService
{
public:
    BootAndCloneService() = default;

    // Clone Operations
    Status TransferBootArea(NativeFileSystemSession* source, NativeFileSystemSession* target);
    Status TransferFiles(NativeFileSystemSession* source, NativeFileSystemSession* target, const std::vector<std::string>& file_names);

    // Boot Info Operations
    Result<BootInfoSummary> GetBootInfoSummary(const NativeFileSystemSession* session);
};
} // namespace legacy89diskkit::cpp::application
