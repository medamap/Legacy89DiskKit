#pragma once

#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <memory>
#include <optional>
#include <span>
#include <string>
#include <vector>

namespace legacy89diskkit
{
namespace cpp
{
namespace application
{
class DiskService
{
public:
    DiskService() = default;
    ~DiskService() = default;

    DiskService(const DiskService&) = delete;
    DiskService& operator=(const DiskService&) = delete;
    DiskService(DiskService&&) = delete;
    DiskService& operator=(DiskService&&) = delete;

    Status OpenDisk(
        const std::string& file_path,
        bool read_only = true,
        std::optional<FileSystemFamily> explicit_family = std::nullopt);

    Status OpenDiskFromBuffer(
        std::span<const std::uint8_t> buffer,
        bool read_only = true,
        std::optional<BufferDiskImageFormat> format_hint = std::nullopt,
        std::optional<FileSystemFamily> explicit_family = std::nullopt);

    Status CreateDisk(
        const std::string& file_path,
        DiskType type,
        const std::string& name = "");

    Status Format();

    Status Save();

    void CloseDisk();


    std::optional<DiskContainerMetadata> GetContainerMetadata() const;

    NativeFileSystemSession* GetSession() const;

    bool IsDiskOpen() const;

private:
    std::unique_ptr<NativeFileSystemSession> session_;
};
} // namespace application
} // namespace cpp
} // namespace legacy89diskkit
