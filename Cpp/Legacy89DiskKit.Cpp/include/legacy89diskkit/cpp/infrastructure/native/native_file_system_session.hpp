#pragma once

#include "legacy89diskkit/cpp/domain/directory_layout_types.hpp"
#include "legacy89diskkit/cpp/filesystem_surface_catalog.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/buffer_image_format.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/hu_basic/hu_basic_file_system.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/msx_dos/msx_dos_file_system.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/n88_basic/n88_basic_file_system.hpp"
#include "legacy89diskkit/cpp/status.hpp"

#include <filesystem>
#include <optional>
#include <span>
#include <string>
#include <variant>
#include <vector>

namespace legacy89diskkit::cpp
{
struct NativeBridgeFileEntry
{
    std::string file_name;
    std::string extension;
    std::uint32_t size;
    std::uint16_t load_address;
    std::uint16_t execution_address;
    std::uint16_t attributes;
};

struct NativeBridgeFileSystemInfo
{
    std::string file_system_name;
    std::string platform_id;
    std::int64_t total_capacity;
    std::int64_t free_space;
    int cluster_size;
    int reserved_sectors;
};

class NativeFileSystemSession
{
public:
    NativeFileSystemSession() = default;

    static Result<NativeFileSystemSession> Open(
        const std::filesystem::path& image_path,
        bool read_only,
        std::optional<FileSystemFamily> explicit_family = std::nullopt);

    static Result<NativeFileSystemSession> Create(
        const std::filesystem::path& image_path,
        DiskType type,
        const std::string& name = "");

    static Result<NativeFileSystemSession> OpenFromBuffer(
        std::span<const std::uint8_t> buffer,
        bool read_only,
        std::optional<BufferDiskImageFormat> format_hint = std::nullopt,
        std::optional<FileSystemFamily> explicit_family = std::nullopt);

    NativeFileSystemSession(NativeFileSystemSession&& other) noexcept;
    NativeFileSystemSession& operator=(NativeFileSystemSession&& other) noexcept;

    const std::string& FilePath() const;
    FileSystemFamily Family() const;
    std::string_view FileSystemName() const;
    bool IsReadOnly() const;

    DiskContainerMetadata GetContainerMetadata() const;
    NativeBridgeFileSystemInfo GetFileSystemInfo() const;
    std::vector<NativeBridgeFileEntry> GetFiles() const;
    bool FileExists(std::string_view file_name) const;
    Result<std::vector<std::uint8_t>> ReadFile(std::string_view file_name) const;

    Status WriteFile(
        std::string_view file_name,
        const std::vector<std::uint8_t>& data,
        std::uint16_t attributes = 0);

    Status DeleteFile(std::string_view file_name);
    Status RenameFile(std::string_view old_name, std::string_view new_name);
    Status UpdateAttributes(std::string_view file_name, std::uint16_t attributes);
    Status Format();
    Status Save();

    Result<DirectoryLayout> ReadDirectoryLayout() const;
    Status ApplyDirectoryLayout(const DirectoryLayout& layout);

    ~NativeFileSystemSession();

private:
    using ContainerVariant = std::variant<std::monostate, RawDiskContainer, D88DiskContainer>;
    using FileSystemVariant = std::variant<std::monostate, HuBasicFileSystem, N88BasicFileSystem, MsxDosFileSystem>;

    NativeFileSystemSession(
        std::string file_path,
        FileSystemFamily family,
        ContainerVariant container,
        FileSystemVariant file_system);

    static Result<FileSystemVariant> OpenDetectedFileSystem(
        FileSystemFamily family,
        ContainerVariant& container);

    static Result<FileSystemVariant> DetectAndOpenFileSystem(ContainerVariant& container);

    void RelinkFileSystem();

    std::string file_path_;
    FileSystemFamily family_;
    ContainerVariant container_;
    FileSystemVariant file_system_;
};
}
