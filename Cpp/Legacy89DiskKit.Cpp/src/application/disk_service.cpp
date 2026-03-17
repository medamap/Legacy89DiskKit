#include "legacy89diskkit/cpp/application/disk_service.hpp"

namespace legacy89diskkit
{
namespace cpp
{
namespace application
{
Status DiskService::OpenDisk(
    const std::string& file_path,
    const bool read_only,
    const std::optional<FileSystemFamily> explicit_family)
{
    CloseDisk();

    auto result = NativeFileSystemSession::Open(file_path, read_only, explicit_family);
    if (!result.ok())
    {
        return result.status();
    }

    session_ = std::make_unique<NativeFileSystemSession>(std::move(result.value()));
    return Status::OkStatus();
}

Status DiskService::OpenDiskFromBuffer(
    const std::span<const std::uint8_t> buffer,
    const bool read_only,
    const std::optional<BufferDiskImageFormat> format_hint,
    const std::optional<FileSystemFamily> explicit_family)
{
    CloseDisk();

    auto result = NativeFileSystemSession::OpenFromBuffer(buffer, read_only, format_hint, explicit_family);
    if (!result.ok())
    {
        return result.status();
    }

    session_ = std::make_unique<NativeFileSystemSession>(std::move(result.value()));
    return Status::OkStatus();
}

Status DiskService::CreateDisk(
    const std::string& file_path,
    DiskType type,
    const std::string& name)
{
    CloseDisk();

    auto result = NativeFileSystemSession::Create(file_path, type, name);
    if (!result.ok())
    {
        return result.status();
    }

    session_ = std::make_unique<NativeFileSystemSession>(std::move(result.value()));
    return Status::OkStatus();
}

Status DiskService::Format()
{
    if (!session_)
    {
        return {StatusCode::InvalidArgument, "No disk open to format."};
    }

    return session_->Format();
}

void DiskService::CloseDisk()
{
    session_.reset();
}

std::optional<DiskContainerMetadata> DiskService::GetContainerMetadata() const
{
    if (!session_)
    {
        return std::nullopt;
    }
    return session_->GetContainerMetadata();
}

NativeFileSystemSession* DiskService::GetSession() const
{
    return session_.get();
}

bool DiskService::IsDiskOpen() const
{
    return session_ != nullptr;
}
} // namespace application
} // namespace cpp
} // namespace legacy89diskkit
