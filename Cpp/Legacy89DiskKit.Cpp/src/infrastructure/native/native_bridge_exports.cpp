#include "legacy89diskkit/cpp/infrastructure/native/native_bridge_exports.hpp"

#include "legacy89diskkit_native.h"

#include <algorithm>
#include <cstring>
#include <mutex>
#include <string_view>
#include <unordered_map>
#include <cstdint>
#include <span>

namespace legacy89diskkit::cpp::native
{
namespace
{
struct NativeBridgeHandleEntry
{
    NativeFileSystemSession session;
    NativeBridgeHandleMetadata metadata;
};

std::unordered_map<std::int32_t, NativeBridgeHandleEntry>& Entries()
{
    static std::unordered_map<std::int32_t, NativeBridgeHandleEntry> entries;
    return entries;
}

std::mutex& EntriesMutex()
{
    static std::mutex mutex;
    return mutex;
}

std::int32_t& NextHandle()
{
    static std::int32_t next_handle = 1;
    return next_handle;
}

int WriteUtf8(char* buffer, const std::int32_t capacity, const std::string_view value)
{
    if (buffer == nullptr || capacity <= 0)
    {
        return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    }

    const auto length = static_cast<std::int32_t>(std::min<std::size_t>(value.size(), static_cast<std::size_t>(capacity - 1)));
    if (length > 0)
    {
        std::memcpy(buffer, value.data(), static_cast<std::size_t>(length));
    }
    buffer[length] = '\0';
    return length;
}

int RegisterSession(NativeFileSystemSession session, NativeBridgeHandleMetadata metadata)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    const auto handle = NextHandle()++;
    Entries().emplace(handle, NativeBridgeHandleEntry{std::move(session), std::move(metadata)});
    return handle;
}

NativeBridgeHandleEntry* FindEntry(const std::int32_t handle)
{
    auto& entries = Entries();
    const auto iterator = entries.find(handle);
    return iterator == entries.end() ? nullptr : &iterator->second;
}

LdkStatus ToLdkStatus(const StatusCode code)
{
    switch (code)
    {
        case StatusCode::Ok: return LDK_STATUS_SUCCESS;
        case StatusCode::InvalidArgument: return LDK_STATUS_ERROR_INVALID_ARGUMENT;
        case StatusCode::UnsupportedFormat: return LDK_STATUS_ERROR_NOT_IMPLEMENTED;
        case StatusCode::ParseError: return LDK_STATUS_ERROR_GENERIC;
        case StatusCode::OutOfRange: return LDK_STATUS_ERROR_INVALID_ARGUMENT;
        default: return LDK_STATUS_ERROR_GENERIC;
    }
}

int ldkStatusFromStatus(const Status& status)
{
    return ToLdkStatus(status.code);
}

int RegisterHandle(NativeFileSystemSession session, std::string source_operation, bool is_writable)
{
    return RegisterSession(std::move(session), NativeBridgeHandleMetadata{std::move(source_operation), is_writable});
}

} // anonymous namespace

int NativeBridgeExports::OpenDisk(const char* const path, const std::int32_t read_only_flag)
{
    if (path == nullptr || path[0] == '\0')
    {
        return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto opened = NativeFileSystemSession::Open(path, read_only_flag != 0);
    if (!opened.ok())
    {
        return ToLdkStatus(opened.status().code);
    }

    const bool is_writable = !opened.value().IsReadOnly();
    return RegisterSession(
        std::move(opened.value()),
        NativeBridgeHandleMetadata{"open-disk", is_writable});
}

int NativeBridgeExports::OpenDiskFromBuffer(const void* const data, const std::int32_t length, const std::int32_t read_only_flag)
{
    if (data == nullptr || length <= 0)
    {
        return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    }

    const auto buffer = std::span<const std::uint8_t>(static_cast<const std::uint8_t*>(data), static_cast<std::size_t>(length));
    auto opened = NativeFileSystemSession::OpenFromBuffer(buffer, read_only_flag != 0);
    if (!opened.ok())
    {
        return ToLdkStatus(opened.status().code);
    }

    const bool is_writable = !opened.value().IsReadOnly();
    return RegisterSession(
        static_cast<NativeFileSystemSession&&>(opened.value()),
        NativeBridgeHandleMetadata{"open-disk-from-buffer", is_writable});
}

int NativeBridgeExports::CloseDisk(const std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    return Entries().erase(handle) == 0 ? LDK_STATUS_ERROR_INVALID_HANDLE : LDK_STATUS_SUCCESS;
}

int NativeBridgeExports::IsHandleValid(const std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    return Entries().contains(handle) ? 1 : 0;
}

int NativeBridgeExports::GetOpenHandleCount()
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    return static_cast<int>(Entries().size());
}

int NativeBridgeExports::CloseAllHandles()
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    Entries().clear();
    return LDK_STATUS_SUCCESS;
}

int NativeBridgeExports::GetHandleSourceOperation(const std::int32_t handle, char* buffer, const std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    const auto* entry = FindEntry(handle);
    if (entry == nullptr)
    {
        return LDK_STATUS_ERROR_INVALID_HANDLE;
    }

    return WriteUtf8(buffer, capacity, entry->metadata.source_operation);
}

int NativeBridgeExports::GetHandleIsWritable(const std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    const auto* entry = FindEntry(handle);
    if (entry == nullptr)
    {
        return LDK_STATUS_ERROR_INVALID_HANDLE;
    }

    return entry->metadata.is_writable ? 1 : 0;
}

int NativeBridgeExports::GetHandleSummary(const std::int32_t handle, char* buffer, const std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    const auto* entry = FindEntry(handle);
    if (entry == nullptr)
    {
        return LDK_STATUS_ERROR_INVALID_HANDLE;
    }

    const auto summary = entry->metadata.source_operation + ":" + (entry->metadata.is_writable ? "writable" : "read-only");
    return WriteUtf8(buffer, capacity, summary);
}

int NativeBridgeExports::GetBackendKind(char* buffer, const std::int32_t capacity)
{
    return WriteUtf8(buffer, capacity, "cpp-bridge");
}

int NativeBridgeExports::GetBackendImplementation(char* buffer, const std::int32_t capacity)
{
    return WriteUtf8(buffer, capacity, "Legacy89DiskKit.Cpp");
}

int NativeBridgeExports::GetBackendTarget(char* buffer, const std::int32_t capacity)
{
    return WriteUtf8(buffer, capacity, "NativeFileSystemSession");
}

int NativeBridgeExports::GetBackendSummary(char* buffer, const std::int32_t capacity)
{
    return WriteUtf8(buffer, capacity, "cpp-bridge:Legacy89DiskKit.Cpp->NativeFileSystemSession");
}

int NativeBridgeExports::GetAbiVersion() { return 1; }
int NativeBridgeExports::GetCapabilityFlags() { return 0; }
int NativeBridgeExports::GetCapabilitySummary(char* buffer, const std::int32_t capacity) { return WriteUtf8(buffer, capacity, "none"); }
int NativeBridgeExports::GetStatusName(const std::int32_t status_code, char* buffer, const std::int32_t capacity) { return WriteUtf8(buffer, capacity, "unknown"); }
int NativeBridgeExports::GetStatusCount() { return 0; }
int NativeBridgeExports::GetStatusCodeAt(const std::int32_t index) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetStatusNameAt(const std::int32_t index, char* buffer, const std::int32_t capacity) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetSupportedFileSystemCount() { return 0; }
int NativeBridgeExports::GetSupportedFileSystemName(const std::int32_t index, char* buffer, const std::int32_t capacity) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetSupportedPlatformCount() { return 0; }
int NativeBridgeExports::GetSupportedPlatformName(const std::int32_t index, char* buffer, const std::int32_t capacity) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetSupportedImageFormatCount() { return 0; }
int NativeBridgeExports::GetSupportedImageFormatName(const std::int32_t index, char* buffer, const std::int32_t capacity) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetInvalidHandleValue() { return 0; }
int NativeBridgeExports::GetHandleLifecycleSummary(char* buffer, const std::int32_t capacity) { return WriteUtf8(buffer, capacity, "manual"); }
int NativeBridgeExports::GetHandleValueSummary(char* buffer, const std::int32_t capacity) { return WriteUtf8(buffer, capacity, "int32"); }
int NativeBridgeExports::GetBufferStringPolicySummary(char* buffer, const std::int32_t capacity) { return WriteUtf8(buffer, capacity, "null-terminated"); }
int NativeBridgeExports::GetMutationPolicySummary(char* buffer, const std::int32_t capacity) { return WriteUtf8(buffer, capacity, "direct"); }
int NativeBridgeExports::GetExportCount() { return 0; }
int NativeBridgeExports::GetExportNameAt(const std::int32_t index, char* buffer, const std::int32_t capacity) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetExportGroupAt(const std::int32_t index, char* buffer, const std::int32_t capacity) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetMutatingOperationCount() { return 0; }
int NativeBridgeExports::GetMutatingOperationNameAt(const std::int32_t index, char* buffer, const std::int32_t capacity) { return LDK_STATUS_ERROR_NOT_IMPLEMENTED; }
int NativeBridgeExports::GetOpenModeSummary(char* buffer, const std::int32_t capacity) { return WriteUtf8(buffer, capacity, "read/write"); }
int NativeBridgeExports::GetOpenModeCount() { return 2; }
int NativeBridgeExports::GetOpenModeNameAt(const std::int32_t index, char* buffer, const std::int32_t capacity) 
{
    if (index == 0) return WriteUtf8(buffer, capacity, "ReadOnly");
    if (index == 1) return WriteUtf8(buffer, capacity, "ReadWrite");
    return LDK_STATUS_ERROR_NOT_IMPLEMENTED;
}

int NativeBridgeExports::GetFileSystemInfo(std::int32_t handle, void* info_ptr)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (info_ptr == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    auto info = entry->session.GetFileSystemInfo();
    auto* ldk_info = static_cast<LdkFileSystemInfo*>(info_ptr);
    
    std::memset(ldk_info, 0, sizeof(LdkFileSystemInfo));
    WriteUtf8(ldk_info->file_system_name, sizeof(ldk_info->file_system_name), info.file_system_name);
    WriteUtf8(ldk_info->platform_id, sizeof(ldk_info->platform_id), info.platform_id);
    ldk_info->total_capacity = info.total_capacity;
    ldk_info->free_space = info.free_space;
    ldk_info->cluster_size = info.cluster_size;
    ldk_info->reserved_sectors = info.reserved_sectors;

    return LDK_STATUS_SUCCESS;
}

int NativeBridgeExports::GetContainerMetadata(std::int32_t handle, void* metadata_ptr)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (metadata_ptr == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    auto metadata = entry->session.GetContainerMetadata();
    auto* ldk_metadata = static_cast<LdkDiskContainerMetadata*>(metadata_ptr);

    std::memset(ldk_metadata, 0, sizeof(LdkDiskContainerMetadata));
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
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (out_count == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    *out_count = static_cast<int32_t>(entry->session.GetFiles().size());
    return LDK_STATUS_SUCCESS;
}

int NativeBridgeExports::GetFiles(std::int32_t handle, void* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (buffer == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

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
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (name == nullptr || buffer == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    auto result = entry->session.ReadFile(name);
    if (!result.ok()) return ldkStatusFromStatus(result.status());

    const auto& data = result.value();
    const int32_t size = std::min(static_cast<int32_t>(data.size()), capacity);
    if (size > 0)
    {
        std::memcpy(buffer, data.data(), static_cast<size_t>(size));
    }
    return size;
}

int NativeBridgeExports::DeleteFile(std::int32_t handle, const char* name)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (name == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    return ldkStatusFromStatus(entry->session.DeleteFile(name));
}

int NativeBridgeExports::WriteFile(std::int32_t handle, const char* name, const void* data, std::int32_t length, std::uint16_t attributes)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (name == nullptr || (data == nullptr && length > 0)) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    std::vector<std::uint8_t> bytes;
    if (data != nullptr && length > 0)
    {
        bytes.assign(static_cast<const uint8_t*>(data), static_cast<const uint8_t*>(data) + length);
    }
    
    return ldkStatusFromStatus(entry->session.WriteFile(name, bytes, attributes));
}

int NativeBridgeExports::RenameFile(std::int32_t handle, const char* old_name, const char* new_name)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (old_name == nullptr || new_name == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    return ldkStatusFromStatus(entry->session.RenameFile(old_name, new_name));
}

int NativeBridgeExports::UpdateAttributes(std::int32_t handle, const char* name, std::uint16_t attributes)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (name == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    return ldkStatusFromStatus(entry->session.UpdateAttributes(name, attributes));
}

int NativeBridgeExports::ReadBootArea(std::int32_t handle, void* buffer, std::int32_t capacity)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (buffer == nullptr) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    auto result = entry->session.ReadBootArea();
    if (!result.ok()) return ldkStatusFromStatus(result.status());

    const auto& data = result.value();
    const int32_t size = std::min(static_cast<int32_t>(data.size()), capacity);
    if (size > 0)
    {
        std::memcpy(buffer, data.data(), static_cast<size_t>(size));
    }
    return size;
}

int NativeBridgeExports::WriteBootArea(std::int32_t handle, const void* data, std::int32_t length)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;
    if (data == nullptr && length > 0) return LDK_STATUS_ERROR_INVALID_ARGUMENT;

    std::vector<std::uint8_t> bytes;
    if (data != nullptr && length > 0)
    {
        bytes.assign(static_cast<const uint8_t*>(data), static_cast<const uint8_t*>(data) + length);
    }
    
    return ldkStatusFromStatus(entry->session.WriteBootArea(bytes));
}

int NativeBridgeExports::Format(std::int32_t handle)
{
    std::lock_guard<std::mutex> lock(EntriesMutex());
    auto* entry = FindEntry(handle);
    if (entry == nullptr) return LDK_STATUS_ERROR_INVALID_HANDLE;

    return ldkStatusFromStatus(entry->session.Format());
}

int NativeBridgeExports::CreateDisk(const char* path, std::int32_t disk_type, const char* name)
{
    if (path == nullptr || std::string_view(path).empty())
    {
        return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto result = NativeFileSystemSession::Create(path, static_cast<DiskType>(disk_type), name ? name : "");
    if (!result.ok())
    {
        return ldkStatusFromStatus(result.status());
    }

    return RegisterHandle(std::move(result.value()), std::string("create:") + path, true);
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

LDK_API std::int32_t LDK_CALL ldk_get_export_name_at(int32_t index, char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetExportNameAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_export_group_at(int32_t index, char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetExportGroupAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_mutating_operation_count(void)
{
    return NativeBridgeExports::GetMutatingOperationCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_mutating_operation_name_at(int32_t index, char* buffer, std::int32_t capacity)
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

LDK_API std::int32_t LDK_CALL ldk_get_open_mode_name_at(int32_t index, char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetOpenModeNameAt(index, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_is_handle_valid(std::int32_t handle)
{
    return NativeBridgeExports::IsHandleValid(handle);
}

LDK_API std::int32_t LDK_CALL ldk_get_open_handle_count(void)
{
    return NativeBridgeExports::GetOpenHandleCount();
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_source_operation(std::int32_t handle, char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetHandleSourceOperation(handle, buffer, capacity);
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_is_writable(std::int32_t handle)
{
    return NativeBridgeExports::GetHandleIsWritable(handle);
}

LDK_API std::int32_t LDK_CALL ldk_get_handle_summary(std::int32_t handle, char* buffer, std::int32_t capacity)
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

LDK_API std::int32_t LDK_CALL ldk_delete_file(int32_t handle, const char* name)
{
    return NativeBridgeExports::DeleteFile(handle, name);
}

LDK_API std::int32_t LDK_CALL ldk_write_file(int32_t handle, const char* name, const void* data, int32_t length, uint16_t attributes)
{
    return NativeBridgeExports::WriteFile(handle, name, data, length, attributes);
}

LDK_API std::int32_t LDK_CALL ldk_rename_file(int32_t handle, const char* old_name, const char* new_name)
{
    return NativeBridgeExports::RenameFile(handle, old_name, new_name);
}

LDK_API std::int32_t LDK_CALL ldk_update_attributes(int32_t handle, const char* name, uint16_t attributes)
{
    return NativeBridgeExports::UpdateAttributes(handle, name, attributes);
}

LDK_API std::int32_t LDK_CALL ldk_read_boot_area(int32_t handle, void* buffer, std::int32_t capacity)
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
}
