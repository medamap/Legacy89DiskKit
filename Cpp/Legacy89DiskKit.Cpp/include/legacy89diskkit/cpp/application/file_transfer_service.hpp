#pragma once

#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"
#include "legacy89diskkit/cpp/application/character_encoding_service.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <string>
#include <string_view>
#include <vector>

namespace legacy89diskkit::cpp::application
{
class FileTransferService
{
public:
    explicit FileTransferService(NativeFileSystemSession* session);

    Status ImportFile(
        const std::string& host_path,
        const std::string& disk_file_name,
        bool is_ascii = true);

    Status ExportFile(
        const std::string& disk_file_name,
        const std::string& host_path,
        std::optional<std::string_view> newline_override = std::nullopt);

private:
    NativeFileSystemSession* session_;
    CharacterEncodingService encoding_service_;

    std::string GetDefaultEncodingId() const;
};
} // namespace legacy89diskkit::cpp::application
