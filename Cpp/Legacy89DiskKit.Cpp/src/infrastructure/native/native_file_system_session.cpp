#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include "legacy89diskkit/cpp/hu_basic_shell.hpp"
#include "legacy89diskkit/cpp/hu_basic_virtual_label_entry_rules.hpp"
#include "legacy89diskkit/cpp/infrastructure/disk_image/buffer_image_format.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/explicit_filesystem_selector.hpp"
#include "legacy89diskkit/cpp/infrastructure/filesystem/filesystem_detection.hpp"

#include <algorithm>
#include <cctype>
#include <fstream>
#include <iostream>

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

Status WriteAllBytes(const std::filesystem::path& path, const std::vector<std::uint8_t>& data)
{
    std::ofstream stream(path, std::ios::binary);
    if (!stream)
    {
        return {StatusCode::InvalidArgument, "Could not open file for writing."};
    }
    stream.write(reinterpret_cast<const char*>(data.data()), static_cast<std::streamsize>(data.size()));
    if (!stream)
    {
        return {StatusCode::InvalidArgument, "Failed to write data to file."};
    }
    return Status::OkStatus();
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

Result<NativeFileSystemSession> NativeFileSystemSession::Create(
    const std::filesystem::path& image_path,
    DiskType type,
    const std::string& name)
{
    const auto ext = image_path.extension().string();
    std::string ext_lower;
    for (const auto ch : ext)
    {
        ext_lower.push_back(static_cast<char>(std::tolower(static_cast<unsigned char>(ch))));
    }

    ContainerVariant container_variant;
    std::vector<std::uint8_t> initial_data;
    if (ext_lower == ".d88")
    {
        auto container = D88DiskContainer::CreateNew(type, name);
        if (!container.ok())
        {
            return Result<NativeFileSystemSession>::Failure(container.status().code, container.status().message);
        }
        initial_data = container.value().ToImageData();
        container_variant = ContainerVariant{std::in_place_index<2>, std::move(container.value())};
    }
    else
    {
        auto raw = RawDiskContainer::CreateNew(type);
        if (!raw.ok())
        {
            return Result<NativeFileSystemSession>::Failure(raw.status().code, raw.status().message);
        }
        initial_data = raw.value().ToImageData();
        container_variant = ContainerVariant{std::in_place_index<1>, std::move(raw.value())};
    }

    const auto write_status = WriteAllBytes(image_path, initial_data);
    if (!write_status.ok())
    {
        return Result<NativeFileSystemSession>::Failure(write_status.code, write_status.message);
    }

    auto fs_result = DetectAndOpenFileSystem(container_variant);
    
    FileSystemFamily family = FileSystemFamily::HuBasic;
    FileSystemVariant fs_variant = std::monostate{};

    if (fs_result.ok())
    {
        fs_variant = std::move(fs_result.value());
        family = std::visit([](const auto& fs) -> FileSystemFamily {
            using T = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<T, HuBasicFileSystem>) return FileSystemFamily::HuBasic;
            if constexpr (std::is_same_v<T, N88BasicFileSystem>) return FileSystemFamily::N88Basic;
            if constexpr (std::is_same_v<T, MsxDosFileSystem>) return FileSystemFamily::MsxDos;
            return FileSystemFamily::HuBasic;
        }, fs_variant);
    }
    else
    {
        auto fallback = OpenDetectedFileSystem(FileSystemFamily::HuBasic, container_variant);
        if (fallback.ok())
        {
            fs_variant = std::move(fallback.value());
            family = FileSystemFamily::HuBasic;
        }
    }

    return Result<NativeFileSystemSession>::Success(
        NativeFileSystemSession(image_path.string(), family, std::move(container_variant), std::move(fs_variant)));
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
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return std::vector<NativeBridgeFileEntry>{};
            }
            else
            {
                std::vector<NativeBridgeFileEntry> files;
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
    const std::uint16_t attributes,
    const std::optional<std::uint16_t> load_address,
    const std::optional<std::uint16_t> execution_address)
{
    return std::visit(
        [file_name, &data, attributes, load_address, execution_address](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {StatusCode::InvalidArgument, "Native session is not initialized."};
            }
            else if constexpr (std::is_same_v<TFileSystem, HuBasicFileSystem>)
            {
                return fs.WriteFile(file_name, data, ToHuAttributes(attributes), load_address.value_or(0), execution_address.value_or(0));
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
    const auto format_status = std::visit(
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

    if (!format_status.ok())
    {
        return format_status;
    }

    auto fs_result = OpenDetectedFileSystem(family_, container_);
    if (fs_result.ok())
    {
        file_system_ = std::move(fs_result.value());
    }

    return Save();
}

Result<DirectoryLayout> NativeFileSystemSession::ReadDirectoryLayout() const
{
    return std::visit(
        [](const auto& fs) -> Result<DirectoryLayout>
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, HuBasicFileSystem>)
            {
                const auto hu_layout = fs.ReadDirectoryLayout();
                DirectoryLayout layout;
                for (const auto& item : hu_layout.items)
                {
                    DirectoryLayoutItem common_item;
                    common_item.id = item.id;
                    common_item.order = item.order;
                    common_item.display_name = item.display_name;
                    common_item.kind = (item.kind == HuBasicDirectoryLayoutItemKind::FileEntry) 
                        ? DirectoryLayoutItemKind::FileEntry 
                        : DirectoryLayoutItemKind::VirtualLabel;
                    layout.items.push_back(std::move(common_item));
                }
                return Result<DirectoryLayout>::Success(std::move(layout));
            }
            else
            {
                return Result<DirectoryLayout>::Failure(StatusCode::UnsupportedFormat, "Directory layout is not supported for this file system.");
            }
        },
        file_system_);
}

Status NativeFileSystemSession::ApplyDirectoryLayout(const DirectoryLayout& layout)
{
    return std::visit(
        [&layout](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, HuBasicFileSystem>)
            {
                HuBasicDirectoryLayout hu_layout;
                const auto original_hu_layout = fs.ReadDirectoryLayout();

                for (const auto& common_item : layout.items)
                {
                    auto it = std::find_if(original_hu_layout.items.begin(), original_hu_layout.items.end(), 
                        [&](const auto& candidate) { return candidate.id == common_item.id; });

                    if (it != original_hu_layout.items.end())
                    {
                        hu_layout.items.push_back(*it);
                    }
                    else if (common_item.kind == DirectoryLayoutItemKind::VirtualLabel)
                    {
                        // Create a new Hu-BASIC label entry
                        const auto& name = common_item.display_name;
                        std::string file_name = name.substr(0, std::min<size_t>(name.length(), 13));
                        std::string extension = (name.length() > 13) ? name.substr(13, std::min<size_t>(name.length() - 13, 3)) : "";

                        auto new_entry = HuBasicVirtualLabelEntryRules::CreateEntry(
                            file_name,
                            extension,
                            0x03, // ASCII mode for labels
                            0xFF, // Password flag
                            0,    // Size
                            0xFFFF, // Load address
                            0xFFFF, // End address
                            0xFFFF, // Execution address
                            0x7FFF  // Special cluster for labels
                        );

                        HuBasicDirectoryLayoutItem hu_item;
                        hu_item.id = common_item.id;
                        hu_item.kind = HuBasicDirectoryLayoutItemKind::VirtualLabel;
                        hu_item.display_name = name;
                        hu_item.entry = std::move(new_entry);
                        hu_layout.items.push_back(std::move(hu_item));
                    }
                }

                return fs.ApplyDirectoryLayout(hu_layout);
            }
            else
            {
                return {StatusCode::UnsupportedFormat, "Directory layout is not supported for this file system."};
            }
        },
        file_system_);
}

Status NativeFileSystemSession::Save()
{
    if (file_path_ == "memory-buffer" || file_path_.empty())
    {
        return Status::OkStatus();
    }

    if (IsReadOnly())
    {
        return Status::OkStatus();
    }

    const bool has_changes = std::visit(
        [](const auto& container) -> bool
        {
            using T = std::decay_t<decltype(container)>;
            if constexpr (std::is_same_v<T, std::monostate>)
            {
                return false;
            }
            else
            {
                return container.HasChanges();
            }
        },
        container_);

    if (!has_changes)
    {
        return Status::OkStatus();
    }

    const auto image_data = std::visit(
        [](const auto& container) -> std::vector<std::uint8_t>
        {
            using T = std::decay_t<decltype(container)>;
            if constexpr (std::is_same_v<T, std::monostate>)
            {
                return std::vector<std::uint8_t>{};
            }
            else
            {
                return container.ToImageData();
            }
        },
        container_);
    
    if (image_data.empty())
    {
        return Status::OkStatus();
    }

    auto status = WriteAllBytes(file_path_, image_data);
    if (status.ok())
    {
        std::visit(
            [](auto& container)
            {
                using T = std::decay_t<decltype(container)>;
                if constexpr (!std::is_same_v<T, std::monostate>)
                {
                    container.ResetChanges();
                }
            },
            container_);
    }
    return status;
}

Result<std::vector<std::uint8_t>> NativeFileSystemSession::ReadBootArea() const
{
    return std::visit(
        [](const auto& fs) -> Result<std::vector<std::uint8_t>>
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return Result<std::vector<std::uint8_t>>::Failure(StatusCode::InvalidArgument, "Native session is not initialized.");
            }
            else
            {
                return fs.ReadBootArea();
            }
        },
        file_system_);
}

Status NativeFileSystemSession::WriteBootArea(const std::vector<std::uint8_t>& data)
{
    return std::visit(
        [&data](auto& fs) -> Status
        {
            using TFileSystem = std::decay_t<decltype(fs)>;
            if constexpr (std::is_same_v<TFileSystem, std::monostate>)
            {
                return {StatusCode::InvalidArgument, "Native session is not initialized."};
            }
            else
            {
                return fs.WriteBootArea(data);
            }
        },
        file_system_);
}

NativeFileSystemSession::~NativeFileSystemSession()
{
    Save();
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

NativeFileSystemSession::NativeFileSystemSession(NativeFileSystemSession&& other) noexcept
    : file_path_(std::move(other.file_path_)),
      family_(other.family_),
      container_(std::move(other.container_)),
      file_system_(std::move(other.file_system_))
{
    RelinkFileSystem();
}

NativeFileSystemSession& NativeFileSystemSession::operator=(NativeFileSystemSession&& other) noexcept
{
    if (this != &other)
    {
        file_path_ = std::move(other.file_path_);
        family_ = other.family_;
        container_ = std::move(other.container_);
        file_system_ = std::move(other.file_system_);
        RelinkFileSystem();
    }
    return *this;
}

void NativeFileSystemSession::RelinkFileSystem()
{
    std::visit(
        [this](auto& container)
        {
            using TContainer = std::decay_t<decltype(container)>;
            if constexpr (!std::is_same_v<TContainer, std::monostate>)
            {
                std::visit(
                    [&container](auto& fs)
                    {
                        using TFS = std::decay_t<decltype(fs)>;
                        if constexpr (std::is_same_v<TFS, HuBasicFileSystem>)
                        {
                            fs = HuBasicFileSystem::Open(container);
                        }
                        else if constexpr (std::is_same_v<TFS, N88BasicFileSystem>)
                        {
                            fs = N88BasicFileSystem::Open(container);
                        }
                        else if constexpr (std::is_same_v<TFS, MsxDosFileSystem>)
                        {
                            fs = MsxDosFileSystem::OpenExplicit(container);
                        }
                    },
                    file_system_);
            }
        },
        container_);
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
