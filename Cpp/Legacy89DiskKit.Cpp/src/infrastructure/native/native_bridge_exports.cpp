#include "legacy89diskkit/cpp/infrastructure/native/native_bridge_exports.hpp"

#include "legacy89diskkit_native.h"

#include <algorithm>
#include <cstring>
#include <mutex>
#include <string_view>
#include <unordered_map>

namespace legacy89diskkit::cpp
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
}

int NativeBridgeExports::OpenDisk(const char* path, const std::int32_t read_only_flag)
{
    if (path == nullptr || path[0] == '\0')
    {
        return LDK_STATUS_ERROR_INVALID_ARGUMENT;
    }

    const auto opened = NativeFileSystemSession::Open(path, read_only_flag != 0);
    if (!opened.ok())
    {
        return LDK_STATUS_ERROR_GENERIC;
    }

    return RegisterSession(
        opened.value(),
        NativeBridgeHandleMetadata{"open-disk", !opened.value().IsReadOnly()});
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

extern "C"
{
std::int32_t LDK_CALL ldk_open_disk(const char* path, std::int32_t read_only_flag)
{
    return NativeBridgeExports::OpenDisk(path, read_only_flag);
}

std::int32_t LDK_CALL ldk_close_disk(std::int32_t handle)
{
    return NativeBridgeExports::CloseDisk(handle);
}

std::int32_t LDK_CALL ldk_is_handle_valid(std::int32_t handle)
{
    return NativeBridgeExports::IsHandleValid(handle);
}

std::int32_t LDK_CALL ldk_get_open_handle_count(void)
{
    return NativeBridgeExports::GetOpenHandleCount();
}

std::int32_t LDK_CALL ldk_close_all_handles(void)
{
    return NativeBridgeExports::CloseAllHandles();
}

std::int32_t LDK_CALL ldk_get_handle_source_operation(std::int32_t handle, char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetHandleSourceOperation(handle, buffer, capacity);
}

std::int32_t LDK_CALL ldk_get_handle_is_writable(std::int32_t handle)
{
    return NativeBridgeExports::GetHandleIsWritable(handle);
}

std::int32_t LDK_CALL ldk_get_handle_summary(std::int32_t handle, char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetHandleSummary(handle, buffer, capacity);
}

std::int32_t LDK_CALL ldk_get_backend_kind(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendKind(buffer, capacity);
}

std::int32_t LDK_CALL ldk_get_backend_implementation(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendImplementation(buffer, capacity);
}

std::int32_t LDK_CALL ldk_get_backend_target(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendTarget(buffer, capacity);
}

std::int32_t LDK_CALL ldk_get_backend_summary(char* buffer, std::int32_t capacity)
{
    return NativeBridgeExports::GetBackendSummary(buffer, capacity);
}
}
}
