#include "legacy89diskkit/cpp/infrastructure/native/native_bridge_exports.hpp"

#include <algorithm>
#include <cstring>
#include <memory>
#include <mutex>
#include <vector>
#include <map>

#include "legacy89diskkit_native.h"

namespace legacy89diskkit::cpp::native
{
namespace
{
struct HandleEntry
{
    NativeFileSystemSession session;
    std::string source_operation;
    bool is_writable;
};

std::mutex& EntriesMutex()
{
    static std::mutex mutex;
    return mutex;
}

std::map<int32_t, HandleEntry>& Entries()
{
    static std::map<int32_t, HandleEntry> entries;
    return entries;
}

int32_t& NextHandle()
{
    static int32_t next = 1;
    return next;
}

HandleEntry* FindEntry(int32_t handle)
{
    auto& entries = Entries();
    auto it = entries.find(handle);
    return (it != entries.end()) ? &it->second : nullptr;
}

int32_t RegisterHandle(NativeFileSystemSession session, std::string source_op, bool writable)
{
    auto& entries = Entries();
    int32_t handle = NextHandle()++;
    entries.emplace(handle, HandleEntry{std::move(session), std::move(source_op), writable});
    return handle;
}

LdkStatus LdkStatusFromStatus(const Status& status)
{
    if (status.ok()) return LDK_STATUS_SUCCESS;
    switch (status.code)
    {
    case StatusCode::InvalidArgument: return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    case StatusCode::UnsupportedFormat: return LDK_STATUS_ERROR_NOT_IMPLEMENTED;
    case StatusCode::ParseError: return LDK_STATUS_ERROR_GENERIC;
    case StatusCode::OutOfRange: return LDK_STATUS_ERROR_BUFFER_TOO_SMALL;
    default: return LDK_STATUS_ERROR_GENERIC;
    }
}

int32_t WriteUtf8(char* buffer, int32_t capacity, std::string_view text)
{
    if (buffer == nullptr || capacity <= 0) return 0;
    int32_t length = static_cast<int32_t>(std::min<size_t>(text.length(), static_cast<size_t>(capacity - 1)));
    std::memcpy(buffer, text.data(), static_cast<size_t>(length));
    buffer[length] = '\0';
    return length;
}

HuBasicFileAttributes ToHuAttributes(const std::uint16_t attributes)
{
    const auto mode = static_cast<std::uint8_t>(attributes & 0xff);
    return {
        (mode & 0x0c) != 0, // is_ascii
        mode,               // raw_attributes
        (mode & 0x80) != 0, // is_directory
        (mode & 0x40) != 0, // is_read_only
        (mode & 0x10) != 0  // is_hidden
    };
}

N88BasicFileAttributes ToN88Attributes(const std::uint16_t attributes)
{
    const auto mode = static_cast<std::uint8_t>(attributes & 0xff);
    return {
        (mode & 0x0c) != 0, // is_ascii
        mode,               // raw_attributes
        (mode & 0x10) != 0  // is_read_only (Changed from 0x40 to 0x10)
    };
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
} // namespace

int NativeBridgeExports::OpenDisk(const char* path, std::int32_t read_only_flag)
{
    if (path == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    auto result = NativeFileSystemSession::Open(path, read_only_flag != 0);
    if (!result.ok()) return LdkStatusFromStatus(result.status());
    return RegisterHandle(std::move(result.value()), std::string("open:") + path, read_only_flag == 0);
}

int NativeBridgeExports::OpenDiskFromBuffer(const void* data, std::int32_t length, std::int32_t read_only_flag)
{
    if (data == nullptr || length <= 0) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    std::vector<std::uint8_t> buffer(static_cast<const uint8_t*>(data), static_cast<const uint8_t*>(data) + length);
    auto result = NativeFileSystemSession::OpenFromBuffer(buffer, read_only_flag != 0);
    if (!result.ok()) return LdkStatusFromStatus(result.status());
    return RegisterHandle(std::move(result.value()), "open:buffer", read_only_flag == 0);
}

int NativeBridgeExports::CreateDisk(const char* path, std::int32_t disk_type, const char* name)
{
    if (path == nullptr || std::string_view(path).empty()) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    auto result = NativeFileSystemSession::Create(path, static_cast<DiskType>(disk_type), name ? name : "");
    if (!result.ok()) return LdkStatusFromStatus(result.status());
    return RegisterHandle(std::move(result.value()), std::string("create:") + path, true);
}

int NativeBridgeExports::CloseDisk(std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    return (Entries().erase(handle) > 0) ? LDK_STATUS_SUCCESS : LDK_STATUS_ERROR_INVALID_HANDLE;
}

int NativeBridgeExports::GetAbiVersion() { return 1; }
int NativeBridgeExports::GetCapabilityFlags() { return 0; }
int NativeBridgeExports::GetCapabilitySummary(char* buffer, std::int32_t capacity) { return WriteUtf8(buffer, capacity, "C++ Native Core"); }
int NativeBridgeExports::GetStatusName(int32_t status_code, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetStatusCount() { return 0; }
int NativeBridgeExports::GetStatusCodeAt(int32_t index) { return 0; }
int NativeBridgeExports::GetStatusNameAt(int32_t index, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetSupportedFileSystemCount() { return 0; }
int NativeBridgeExports::GetSupportedFileSystemName(int32_t index, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetSupportedPlatformCount() { return 0; }
int NativeBridgeExports::GetSupportedPlatformName(int32_t index, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetSupportedImageFormatCount() { return 0; }
int NativeBridgeExports::GetSupportedImageFormatName(int32_t index, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetInvalidHandleValue() { return -1; }
int NativeBridgeExports::GetHandleLifecycleSummary(char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetHandleValueSummary(char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetBufferStringPolicySummary(char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetMutationPolicySummary(char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetBackendKind(char* buffer, std::int32_t capacity) { return WriteUtf8(buffer, capacity, "native-library"); }
int NativeBridgeExports::GetBackendImplementation(char* buffer, std::int32_t capacity) { return WriteUtf8(buffer, capacity, "libLegacy89DiskKitCpp"); }
int NativeBridgeExports::GetBackendTarget(char* buffer, std::int32_t capacity) { return WriteUtf8(buffer, capacity, "C++ Core"); }
int NativeBridgeExports::GetBackendSummary(char* buffer, std::int32_t capacity) { return WriteUtf8(buffer, capacity, "C++ Core Backend"); }
int NativeBridgeExports::GetExportCount() { return 0; }
int NativeBridgeExports::GetExportNameAt(int32_t index, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetExportGroupAt(int32_t index, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetMutatingOperationCount() { return 0; }
int NativeBridgeExports::GetMutatingOperationNameAt(int32_t index, char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetOpenModeSummary(char* buffer, std::int32_t capacity) { return 0; }
int NativeBridgeExports::GetOpenModeCount() { return 0; }
int NativeBridgeExports::GetOpenModeNameAt(int32_t index, char* buffer, std::int32_t capacity) { return 0; }

int NativeBridgeExports::IsHandleValid(std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    return FindEntry(handle) != nullptr ? 1 : 0;
}

int NativeBridgeExports::GetOpenHandleCount()
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    return static_cast<int32_t>(Entries().size());
}

int NativeBridgeExports::CloseAllHandles()
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    int count = static_cast<int32_t>(Entries().size());
    Entries().clear();
    return count;
}

int NativeBridgeExports::GetHandleSourceOperation(std::int32_t handle, char* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    return entry ? WriteUtf8(buffer, capacity, entry->source_operation) : 0;
}

int NativeBridgeExports::GetHandleIsWritable(std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    return (entry && entry->is_writable) ? 1 : 0;
}

int NativeBridgeExports::GetHandleSummary(std::int32_t handle, char* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    return entry ? WriteUtf8(buffer, capacity, entry->session.FileSystemName()) : 0;
}

int NativeBridgeExports::GetFileSystemInfo(std::int32_t handle, void* info_ptr)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || info_ptr == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    auto info = entry->session.GetFileSystemInfo();
    auto* ldk_info = static_cast<LdkFileSystemInfo*>(info_ptr);
    WriteUtf8(ldk_info->file_system_name, sizeof(ldk_info->file_system_name), info.file_system_name);
    ldk_info->total_capacity = info.total_capacity;
    ldk_info->free_space = info.free_space;
    ldk_info->cluster_size = info.cluster_size;
    ldk_info->reserved_sectors = info.reserved_sectors;
    WriteUtf8(ldk_info->platform_id, sizeof(ldk_info->platform_id), info.platform_id);
    return LDK_STATUS_SUCCESS;
}

int NativeBridgeExports::GetContainerMetadata(std::int32_t handle, void* metadata_ptr)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || metadata_ptr == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    auto metadata = entry->session.GetContainerMetadata();
    auto* ldk_metadata = static_cast<LdkDiskContainerMetadata*>(metadata_ptr);
    WriteUtf8(ldk_metadata->image_format, sizeof(ldk_metadata->image_format), metadata.image_format);
    ldk_metadata->disk_type = static_cast<int32_t>(metadata.disk_type);
    ldk_metadata->cylinders = metadata.geometry.cylinders;
    ldk_metadata->heads = metadata.geometry.heads;
    ldk_metadata->sectors_per_track = metadata.geometry.sectors_per_track;
    ldk_metadata->bytes_per_sector = metadata.geometry.bytes_per_sector;
    ldk_metadata->is_write_protected = metadata.is_write_protected ? 1 : 0;
    ldk_metadata->declared_image_size = metadata.declared_image_size;
    return LDK_STATUS_SUCCESS;
}

int NativeBridgeExports::GetFilesCount(std::int32_t handle, std::int32_t* out_count)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || out_count == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    *out_count = static_cast<std::int32_t>(entry->session.GetFiles().size());
    return LDK_STATUS_SUCCESS;
}

int NativeBridgeExports::GetFiles(std::int32_t handle, void* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (buffer == nullptr || capacity < 0) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    auto files = entry->session.GetFiles();
    auto* ldk_files = static_cast<LdkFileEntry*>(buffer);
    const int32_t count = std::min(static_cast<int32_t>(files.size()), capacity);

    for (int32_t i = 0; i < count; ++i)
    {
        std::memset(&ldk_files[i], 0, sizeof(LdkFileEntry));
        WriteUtf8(ldk_files[i].file_name, sizeof(ldk_files[i].file_name), files[i].file_name);
        WriteUtf8(ldk_files[i].extension, sizeof(ldk_files[i].extension), files[i].extension);
        ldk_files[i].size = static_cast<int32_t>(files[i].size);
        ldk_files[i].load_address = files[i].load_address;
        ldk_files[i].execution_address = files[i].execution_address;
        ldk_files[i].attributes = files[i].attributes;
    }
    return count;
}

int NativeBridgeExports::ReadFile(std::int32_t handle, const char* name, void* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || name == nullptr || buffer == nullptr || capacity < 0) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    auto result = entry->session.ReadFile(name);
    if (!result.ok()) return LdkStatusFromStatus(result.status());
    const auto& data = result.value();
    const int32_t size = std::min(static_cast<int32_t>(data.size()), capacity);
    std::memcpy(buffer, data.data(), static_cast<size_t>(size));
    return size;
}

int NativeBridgeExports::ReadSector(std::int32_t handle, std::int32_t cylinder, std::int32_t head, std::int32_t sector, void* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || buffer == nullptr || capacity < 0) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    auto result = entry->session.ReadSector(cylinder, head, sector);
    if (!result.ok()) return LdkStatusFromStatus(result.status());
    const auto& data = result.value();
    const int32_t size = std::min(static_cast<int32_t>(data.size()), capacity);
    std::memcpy(buffer, data.data(), static_cast<size_t>(size));
    return size;
}

int NativeBridgeExports::DeleteFile(std::int32_t handle, const char* name)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || name == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    return LdkStatusFromStatus(entry->session.DeleteFile(name));
}

int NativeBridgeExports::WriteFile(std::int32_t handle, const char* name, const void* data, std::int32_t length, std::uint16_t attributes, std::uint16_t load_address, std::uint16_t execution_address)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || name == nullptr || (data == nullptr && length > 0)) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    std::vector<std::uint8_t> bytes;
    if (data != nullptr && length > 0) bytes.assign(static_cast<const uint8_t*>(data), static_cast<const uint8_t*>(data) + length);
    return LdkStatusFromStatus(entry->session.WriteFile(name, bytes, attributes, load_address, execution_address));
}

int NativeBridgeExports::WriteSector(std::int32_t handle, std::int32_t cylinder, std::int32_t head, std::int32_t sector, const void* data, std::int32_t length)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || data == nullptr || length < 0) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    std::vector<std::uint8_t> bytes(static_cast<const uint8_t*>(data), static_cast<const uint8_t*>(data) + length);
    return LdkStatusFromStatus(entry->session.WriteSector(cylinder, head, sector, bytes));
}

int NativeBridgeExports::RenameFile(std::int32_t handle, const char* old_name, const char* new_name)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || old_name == nullptr || new_name == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    return LdkStatusFromStatus(entry->session.RenameFile(old_name, new_name));
}

int NativeBridgeExports::UpdateAttributes(std::int32_t handle, const char* name, std::uint16_t attributes)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || name == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    return LdkStatusFromStatus(entry->session.UpdateAttributes(name, attributes));
}

int NativeBridgeExports::ReadBootArea(std::int32_t handle, void* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || buffer == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    auto result = entry->session.ReadBootArea();
    if (!result.ok()) return LdkStatusFromStatus(result.status());
    const auto& data = result.value();
    const int32_t size = std::min(static_cast<int32_t>(data.size()), capacity);
    if (size > 0) std::memcpy(buffer, data.data(), static_cast<size_t>(size));
    return size;
}

int NativeBridgeExports::WriteBootArea(std::int32_t handle, const void* data, std::int32_t length)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr || (data == nullptr && length > 0)) return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    std::vector<std::uint8_t> bytes;
    if (data != nullptr && length > 0) bytes.assign(static_cast<const uint8_t*>(data), static_cast<const uint8_t*>(data) + length);
    return LdkStatusFromStatus(entry->session.WriteBootArea(bytes));
}

int NativeBridgeExports::Format(std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    return LdkStatusFromStatus(entry->session.Format());
}
} // namespace legacy89diskkit::cpp::native

using namespace legacy89diskkit::cpp;
using namespace legacy89diskkit::cpp::native;

extern "C"
{
LDK_API std::int32_t LDK_CALL ldk_open_disk(const char* const path, const std::int32_t read_only_flag)
{
    return NativeBridgeExports::OpenDisk(path, read_only_flag);
}

LDK_API std::int32_t LDK_CALL ldk_open_disk_from_buffer(const void* const data, const std::int32_t length, const std::int32_t read_only_flag)
{
    return NativeBridgeExports::OpenDiskFromBuffer(data, length, read_only_flag);
}

LDK_API std::int32_t LDK_CALL ldk_create_disk(const char* const path, const std::int32_t disk_type, const char* const name)
{
    return NativeBridgeExports::CreateDisk(path, disk_type, name);
}

LDK_API std::int32_t LDK_CALL ldk_close_disk(std::int32_t handle)
{
    return NativeBridgeExports::CloseDisk(handle);
}

LDK_API std::int32_t LDK_CALL ldk_get_abi_version(void)
{
    return NativeBridgeExports::GetAbiVersion();
}

LDK_API std::int32_t LDK_CALL ldk_get_capability_flags(void)
{
    return NativeBridgeExports::GetCapabilityFlags();
}

LDK_API std::int32_t LDK_CALL ldk_get_capability_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetCapabilitySummary(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_status_name(int32_t status_code, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetStatusName(status_code, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_status_count(void)
{
    return NativeBridgeExports::GetStatusCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_status_code_at(int32_t index)
{
    return NativeBridgeExports::GetStatusCodeAt(index);
}

LDK_API std::int32_t LDK_CALL ldk_get_status_name_at(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetStatusNameAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_supported_file_system_count(void)
{
    return NativeBridgeExports::GetSupportedFileSystemCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_supported_file_system_name(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetSupportedFileSystemName(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_supported_platform_count(void)
{
    return NativeBridgeExports::GetSupportedPlatformCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_supported_platform_name(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetSupportedPlatformName(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_supported_image_format_count(void)
{
    return NativeBridgeExports::GetSupportedImageFormatCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_supported_image_format_name(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetSupportedImageFormatName(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_invalid_handle_value(void)
{
    return NativeBridgeExports::GetInvalidHandleValue();
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_lifecycle_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetHandleLifecycleSummary(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_value_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetHandleValueSummary(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_buffer_string_policy_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBufferStringPolicySummary(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_mutation_policy_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetMutationPolicySummary(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_backend_kind(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendKind(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_backend_implementation(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendImplementation(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_backend_target(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendTarget(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_backend_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendSummary(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_export_count(void)
{
    return NativeBridgeExports::GetExportCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_export_name_at(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetExportNameAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_export_group_at(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetExportGroupAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_mutating_operation_count(void)
{
    return NativeBridgeExports::GetMutatingOperationCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_mutating_operation_name_at(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetMutatingOperationNameAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_open_mode_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetOpenModeSummary(buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_open_mode_count(void)
{
    return NativeBridgeExports::GetOpenModeCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_open_mode_name_at(int32_t index, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetOpenModeNameAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_is_handle_valid(int32_t handle)
{
    return NativeBridgeExports::IsHandleValid(handle);
}

LDK_API std::int32_t LDK_CALL ldk_get_open_handle_count(void)
{
    return NativeBridgeExports::GetOpenHandleCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_source_operation(int32_t handle, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetHandleSourceOperation(handle, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_is_writable(int32_t handle)
{
    return NativeBridgeExports::GetHandleIsWritable(handle);
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_summary(int32_t handle, char* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetHandleSummary(handle, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_close_all_handles(void)
{
    return NativeBridgeExports::CloseAllHandles();
}

LDK_API std::int32_t LDK_CALL ldk_get_file_system_info(int32_t handle, LdkFileSystemInfo* info)
{
    return NativeBridgeExports::GetFileSystemInfo(handle, info);
}

LDK_API std::int32_t LDK_CALL ldk_get_container_metadata(int32_t handle, LdkDiskContainerMetadata* metadata)
{
    return NativeBridgeExports::GetContainerMetadata(handle, metadata);
}

LDK_API std::int32_t LDK_CALL ldk_get_files_count(int32_t handle, int32_t* out_count)
{
    return NativeBridgeExports::GetFilesCount(handle, out_count);
}

LDK_API std::int32_t LDK_CALL ldk_get_files(int32_t handle, LdkFileEntry* buffer, int32_t capacity)
{
    return NativeBridgeExports::GetFiles(handle, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_read_file(int32_t handle, const char* name, void* buffer, int32_t capacity)
{
    return NativeBridgeExports::ReadFile(handle, name, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_read_sector(int32_t handle, int32_t cylinder, int32_t head, int32_t sector, void* buffer, int32_t capacity)
{
    return NativeBridgeExports::ReadSector(handle, cylinder, head, sector, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_delete_file(int32_t handle, const char* name)
{
    return NativeBridgeExports::DeleteFile(handle, name);
}

LDK_API std::int32_t LDK_CALL ldk_write_file(int32_t handle, const char* name, const void* data, int32_t length, uint16_t attributes, uint16_t load_address, uint16_t execution_address)
{
    return NativeBridgeExports::WriteFile(handle, name, data, length, attributes, load_address, execution_address);
}

LDK_API std::int32_t LDK_CALL ldk_write_sector(int32_t handle, int32_t cylinder, int32_t head, int32_t sector, const void* data, int32_t length)
{
    return NativeBridgeExports::WriteSector(handle, cylinder, head, sector, data, length);
}

LDK_API std::int32_t LDK_CALL ldk_rename_file(int32_t handle, const char* old_name, const char* new_name)
{
    return NativeBridgeExports::RenameFile(handle, old_name, new_name);
}

LDK_API std::int32_t LDK_CALL ldk_update_attributes(int32_t handle, const char* name, uint16_t attributes)
{
    return NativeBridgeExports::UpdateAttributes(handle, name, attributes);
}

LDK_API std::int32_t LDK_CALL ldk_read_boot_area(int32_t handle, void* buffer, int32_t capacity)
{
    return NativeBridgeExports::ReadBootArea(handle, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_write_boot_area(int32_t handle, const void* data, int32_t length)
{
    return NativeBridgeExports::WriteBootArea(handle, data, length);
}

LDK_API std::int32_t LDK_CALL ldk_format(int32_t handle)
{
    return NativeBridgeExports::Format(handle);
}

} // extern "C"
