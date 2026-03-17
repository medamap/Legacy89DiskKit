#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include "legacy89diskkit/cpp/infrastructure/disk_image/buffer_image_format.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/explicit_filesystem_selector.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/filesystem_detection.hpp"

#include <algorithm>
#include <cctype>
#include <fstream>

namespace legacy89diskkit::cpp
{
namespace
{
std::string ToLower(std::string value)
{
    std::transform(
        value.begin(),
        value.end(),
        value.begin(),
        [](const unsigned char ch)
        {
            return static_cast<char>(std::tolower(ch));
        });
    return value;
}

bool IsD88Path(const std::filesystem::path& image_path)
{
    return ToLower(image_path.extension().string()) == ".d88";
}

std::vector<std::uint8_t> ReadAllBytes(const std::filesystem::path& image_path)
{
    std::ifstream stream(image_path, std::ios::binary);
    return std::vector<std::uint8_t>(std::istreambuf_iterator<char>(stream), std::istreambuf_iterator<char>());
}

NativeBridgeFileEntry ToNativeEntry(const HuBasicFileEntry& entry)
{
    return {
        entry.file_name,
        entry.extension,
        entry.size,
        entry.load_address,
        entry.execution_address,
        entry.attributes.raw_attributes};
}

NativeBridgeFileEntry ToNativeEntry(const N88BasicFileEntry& entry)
{
    return {
        entry.file_name,
        entry.extension,
        entry.size,
        0,
        0,
        entry.attributes.raw_attributes};
}

NativeBridgeFileEntry ToNativeEntry(const MsxDosFileEntry& entry)
{
    return {
        entry.file_name,
        entry.extension,
        entry.size,
        0,
        0,
        entry.attributes.raw_attributes};
}

NativeBridgeFileSystemInfo ToNativeInfo(const HuBasicFileSystemInfo& info)
{
    return {"Hu-BASIC", "X1", info.total_size, info.free_space, info.cluster_size, info.reserved_sectors};
}

NativeBridgeFileSystemInfo ToNativeInfo(const N88BasicFileSystemInfo& info)
{
    return {"N88-BASIC", "PC88", info.total_size, info.free_space, info.cluster_size, info.reserved_clusters};
}

NativeBridgeFileSystemInfo ToNativeInfo(const MsxDosFileSystemInfo& info)
{
    return {"MSX-DOS", "MSX", info.total_size, info.free_space, info.cluster_size, info.first_data_sector};
}

HuBasicFileAttributes ToHuAttributes(const std::uint16_t attributes)
{
    return {
        false,
        static_cast<std::uint8_t>(attributes & 0xff),
        false,
        (attributes & 0x01u) != 0,
        false};
}

N88BasicFileAttributes ToN88Attributes(const std::uint16_t attributes)
{
    return {false, static_cast<std::uint8_t>(attributes & 0xff), (attributes & 0x01u) != 0};
}

MsxDosFileAttributes ToMsxAttributes(const std::uint16_t attributes)
{
    const auto raw = static_cast<std::uint8_t>(attributes & 0xff);
    return {
        false,
        raw,
        (raw & 0x01u) != 0,
        (raw & 0x02u) != 0,
        (raw & 0x04u) != 0,
        (raw & 0x10u) != 0,
        (raw & 0x20u) != 0};
}
}

Result<NativeFileSystemSession> NativeFileSystemSession::Open(
    const std::filesystem::path& image_path,
    const bool read_only,
    const std::optional<FileSystemFamily> explicit_family)
{
    const auto bytes = ReadAllBytes(image_path);
    if (bytes.empty())
    {
        return Result<NativeFileSystemSession>::Failure(StatusCode::InvalidArgument, "Image path could not be read.");
    }

    const auto ext = image_path.extension().string();
    std::string ext_lower;
    for (const auto ch : ext)
    {
        ext_lower.push_back(static_cast<char>(std::tolower(static_cast<unsigned char>(ch))));
    }

    ContainerVariant container_variant;
    if (ext_lower == ".d88")
    {
        auto container = D88DiskContainer::OpenFromBuffer(bytes, read_only);
        if (!container.ok())
        {
            return Result<NativeFileSystemSession>::Failure(container.status().code, container.status().message);
        }
        container_variant = ContainerVariant{std::in_place_index<2>, std::move(container.value())};
    }
    else
    {
        auto raw = RawDiskContainer::OpenFromBuffer(bytes, read_only);
        if (!raw.ok())
        {
            return Result<NativeFileSystemSession>::Failure(raw.status().code, raw.status().message);
        }
        container_variant = ContainerVariant{std::in_place_index<1>, std::move(raw.value())};
    }

    Result<FileSystemVariant> file_system = explicit_family.has_value()
        ? OpenDetectedFileSystem(explicit_family.value(), container_variant)
        : DetectAndOpenFileSystem(container_variant);

    if (!file_system.ok())
    {
        return Result<NativeFileSystemSession>::Failure(file_system.status().code, file_system.status().message);
    }

    const auto family = explicit_family.has_value()
        ? explicit_family.value()
        : std::visit(
            [](const auto& value)
            {
                using TValue = std::decay_t<decltype(value)>;
                if constexpr (std::is_same_v<TValue, HuBasicFileSystem>)
                {
                    return FileSystemFamily::HuBasic;
                }
                if constexpr (std::is_same_v<TValue, N88BasicFileSystem>)
                {
                    return FileSystemFamily::N88Basic;
                }
                return FileSystemFamily::MsxDos;
            },
            file_system.value());

    return Result<NativeFileSystemSession>::Success(
        NativeFileSystemSession(image_path.string(), family, std::move(container_variant), std::move(file_system.value())));
}

Result<NativeFileSystemSession> NativeFileSystemSession::OpenFromBuffer(
    std::span<const std::uint8_t> buffer,
    const bool read_only,
    std::optional<BufferDiskImageFormat> format_hint,
    const std::optional<FileSystemFamily> explicit_family)
{
    ContainerVariant container_variant;
    if (format_hint.has_value() && format_hint.value() == BufferDiskImageFormat::D88)
    {
        auto container = D88DiskContainer::OpenFromBuffer(buffer, read_only);
        if (!container.ok())
        {
            return Result<NativeFileSystemSession>::Failure(container.status().code, container.status().message);
        }
        container_variant = ContainerVariant{std::in_place_index<2>, std::move(container.value())};
    }
    else if (format_hint.has_value() && format_hint.value() == BufferDiskImageFormat::Raw)
    {
        auto raw = RawDiskContainer::OpenFromBuffer(buffer, read_only);
        if (!raw.ok())
        {
            return Result<NativeFileSystemSession>::Failure(raw.status().code, raw.status().message);
        }
        container_variant = ContainerVariant{std::in_place_index<1>, std::move(raw.value())};
    }
    else
    {
        // Auto (std::nullopt): try D88 if size suggests it might have a header, otherwise Raw
        bool d88_success = false;
        if (buffer.size() >= 0x2b0)
        {
            auto container = D88DiskContainer::OpenFromBuffer(buffer, read_only);
            if (container.ok() && !container.value().GetAllSectors().empty())
            {
                container_variant = ContainerVariant{std::in_place_index<2>, std::move(container.value())};
                d88_success = true;
            }
        }

        if (!d88_success)
        {
            auto raw = RawDiskContainer::OpenFromBuffer(buffer, read_only);
            if (!raw.ok())
            {
                return Result<NativeFileSystemSession>::Failure(raw.status().code, raw.status().message);
            }
            container_variant = ContainerVariant{std::in_place_index<1>, std::move(raw.value())};
        }
    }

    Result<FileSystemVariant> file_system = explicit_family.has_value()
        ? OpenDetectedFileSystem(explicit_family.value(), container_variant)
        : DetectAndOpenFileSystem(container_variant);

    if (!file_system.ok())
    {
        return Result<NativeFileSystemSession>::Failure(file_system.status().code, file_system.status().message);
    }

    const auto family = explicit_family.has_value()
        ? explicit_family.value()
        : std::visit(
            [](const auto& value)
            {
                using TValue = std::decay_t<decltype(value)>;
                if constexpr (std::is_same_v<TValue, HuBasicFileSystem>)
                {
                    return FileSystemFamily::HuBasic;
                }
                if constexpr (std::is_same_v<TValue, N88BasicFileSystem>)
                {
                    return FileSystemFamily::N88Basic;
                }
                return FileSystemFamily::MsxDos;
            },
            file_system.value());

    return Result<NativeFileSystemSession>::Success(
        NativeFileSystemSession("memory-buffer", family, std::move(container_variant), std::move(file_system.value())));
}

const std::string& NativeFileSystemSession::FilePath() const
{
    return file_path_;
}

FileSystemFamily NativeFileSystemSession::Family() const
{
    return family_;
}

std::string_view NativeFileSystemSession::FileSystemName() const
{
    const auto* entry = FileSystemSurfaceCatalog::FindByFamily(family_);
    return entry == nullptr ? std::string_view{} : entry->name;
}

bool NativeFileSystemSession::IsReadOnly() const
{
    return std::visit(
        [](const auto& container)
        {
            using TContainer = std::decay_t<decltype(container)>;
            if constexpr (std::is_same_v<TContainer, std::monostate>)
            {
                return true;
            }
            else
            {
                return container.IsReadOnly();
            }
        },
        container_);
}

DiskContainerMetadata NativeFileSystemSession::GetContainerMetadata() const
{
    return std::visit(
        [](const auto& container) -> DiskContainerMetadata
        {
            using TContainer = std::decay_t<decltype(container)>;
            if constexpr (std::is_same_v<TContainer, std::monostate>)
            {
                return {};
            }
            else
            {
                return container.GetMetadata();
            }
        },
        container_);
}

NativeBridgeFileSystemInfo NativeFileSystemSession::GetFileSystemInfo() const
{
    return std::visit(
        [](const auto& fs) -> NativeBridgeFileSystemInfo
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {};
            }
            else
            {
                return ToNativeInfo(fs.GetFileSystemInfo());
            }
        },
        file_system_);
}

std::vector<NativeBridgeFileEntry> NativeFileSystemSession::GetFiles() const
{
    return std::visit(
        [](const auto& fs)
        {
            std::vector<NativeBridgeFileEntry> files;
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return files;
            }
            else
            {
                for (const auto& entry : fs.GetFiles())
                {
                    files.push_back(ToNativeEntry(entry));
                }

                return files;
            }
        },
        file_system_);
}

bool NativeFileSystemSession::FileExists(const std::string_view file_name) const
{
    return std::visit(
        [file_name](const auto& fs)
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return false;
            }
            else
            {
                return fs.FileExists(file_name);
            }
        },
        file_system_);
}

Result<std::vector<std::uint8_t>> NativeFileSystemSession::ReadFile(const std::string_view file_name) const
{
    return std::visit(
        [file_name](const auto& fs) -> Result<std::vector<std::uint8_t>>
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return Result<std::vector<std::uint8_t>>::Failure(StatusCode::InvalidArgument, "Native session is not initialized.");
            }
            else
            {
                return fs.ReadFile(file_name);
            }
        },
        file_system_);
}

Status NativeFileSystemSession::WriteFile(
    const std::string_view file_name,
    const std::vector<std::uint8_t>& data,
    const std::uint16_t attributes)
{
    return std::visit(
        [file_name, &data, attributes](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {StatusCode::InvalidArgument, "Native session is not initialized."};
            }
            else if constexpr (std::is_same_v<TFileSystem, HuBasicFileSystem>)
            {
                return fs.WriteFile(file_name, data, ToHuAttributes(attributes));
            }
            else if constexpr (std::is_same_v<TFileSystem, N88BasicFileSystem>)
            {
                return fs.WriteFile(file_name, data, ToN88Attributes(attributes));
            }
            else
            {
                return fs.WriteFile(file_name, data, ToMsxAttributes(attributes));
            }
        },
        file_system_);
}

Status NativeFileSystemSession::DeleteFile(const std::string_view file_name)
{
    return std::visit(
        [file_name](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {StatusCode::InvalidArgument, "Native session is not initialized."};
            }
            else
            {
                return fs.DeleteFile(file_name);
            }
        },
        file_system_);
}

Status NativeFileSystemSession::RenameFile(const std::string_view old_name, const std::string_view new_name)
{
    return std::visit(
        [old_name, new_name](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {StatusCode::InvalidArgument, "Native session is not initialized."};
            }
            else
            {
                return fs.RenameFile(old_name, new_name);
            }
        },
        file_system_);
}

Status NativeFileSystemSession::UpdateAttributes(const std::string_view file_name, const std::uint16_t attributes)
{
    return std::visit(
        [file_name, attributes](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {StatusCode::InvalidArgument, "Native session is not initialized."};
            }
            else if constexpr (std::is_same_v<TFileSystem, HuBasicFileSystem>)
            {
                return fs.UpdateAttributes(file_name, ToHuAttributes(attributes));
            }
            else if constexpr (std::is_same_v<TFileSystem, N88BasicFileSystem>)
            {
                return fs.UpdateAttributes(file_name, ToN88Attributes(attributes));
            }
            else
            {
                return fs.UpdateAttributes(file_name, ToMsxAttributes(attributes));
            }
        },
        file_system_);
}

Status NativeFileSystemSession::Format()
{
    return std::visit(
        [](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {StatusCode::InvalidArgument, "Native session is not initialized."};
            }
            else
            {
                return fs.Format();
            }
        },
        file_system_);
}

NativeFileSystemSession::NativeFileSystemSession(
    std::string file_path,
    const FileSystemFamily family,
    ContainerVariant container,
    FileSystemVariant file_system)
    : file_path_(std::move(file_path)),
      family_(family),
      container_(std::move(container)),
      file_system_(std::move(file_system))
{
}

Result<NativeFileSystemSession::FileSystemVariant> NativeFileSystemSession::OpenDetectedFileSystem(
    const FileSystemFamily family,
    ContainerVariant& container)
{
    return std::visit(
        [family](auto& value) -> Result<FileSystemVariant>
        {
            using TContainer = std::decay_t<decltype(value)>;
            if constexpr (std::is_same_v<TContainer, std::monostate>)
            {
                return Result<FileSystemVariant>::Failure(StatusCode::InvalidArgument, "Native session container is not initialized.");
            }
            else
            {
                switch (family)
                {
                case FileSystemFamily::HuBasic:
                    return Result<FileSystemVariant>::Success(FileSystemVariant{std::in_place_index<1>, HuBasicFileSystem::Open(value)});
                case FileSystemFamily::N88Basic:
                    return Result<FileSystemVariant>::Success(FileSystemVariant{std::in_place_index<2>, N88BasicFileSystem::Open(value)});
                case FileSystemFamily::MsxDos:
                {
                    auto result = MsxDosFileSystem::OpenExplicit(value);
                    return Result<FileSystemVariant>::Success(FileSystemVariant{std::in_place_index<3>, std::move(result)});
                }
                }

                return Result<FileSystemVariant>::Failure(StatusCode::InvalidArgument, "Unsupported file system.");
            }
        },
        container);
}

Result<NativeFileSystemSession::FileSystemVariant> NativeFileSystemSession::DetectAndOpenFileSystem(ContainerVariant& container)
{
    return std::visit(
        [&container](auto& value) -> Result<FileSystemVariant>
        {
            using TContainer = std::decay_t<decltype(value)>;
            if constexpr (std::is_same_v<TContainer, std::monostate>)
            {
                return Result<FileSystemVariant>::Failure(StatusCode::InvalidArgument, "Native session container is not initialized.");
            }
            else
            {
                const auto detected = FileSystemDetection::DetectBest(value);
                if (detected == nullptr)
                {
                    return Result<FileSystemVariant>::Failure(StatusCode::UnsupportedFormat, "No file system could be detected.");
                }

                return OpenDetectedFileSystem(detected->family, container);
            }
        },
        container);
}
}
