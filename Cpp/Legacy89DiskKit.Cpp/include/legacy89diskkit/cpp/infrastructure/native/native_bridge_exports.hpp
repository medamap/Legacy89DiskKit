#pragma once

#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp::native
{
struct NativeBridgeHandleMetadata
{
    std::string source_operation;
    bool is_writable;
};

class NativeBridgeExports
{
public:
    static int OpenDisk(const char* path, std::int32_t read_only_flag);
    static int OpenDiskFromBuffer(const void* data, std::int32_t length, std::int32_t read_only_flag);
    static int CreateDisk(const char* path, std::int32_t disk_type, const char* name);
    static int CloseDisk(std::int32_t handle);
    static int IsHandleValid(std::int32_t handle);
    static int GetOpenHandleCount();
    static int CloseAllHandles();
    static int GetHandleSourceOperation(std::int32_t handle, char* buffer, std::int32_t capacity);
    static int GetHandleIsWritable(std::int32_t handle);
    static int GetHandleSummary(std::int32_t handle, char* buffer, std::int32_t capacity);
    static int GetBackendKind(char* buffer, std::int32_t capacity);
    static int GetBackendImplementation(char* buffer, std::int32_t capacity);
    static int GetBackendTarget(char* buffer, std::int32_t capacity);
    static int GetBackendSummary(char* buffer, std::int32_t capacity);

    // Metadata and Capabilities
    static int GetAbiVersion();
    static int GetCapabilityFlags();
    static int GetCapabilitySummary(char* buffer, std::int32_t capacity);
    static int GetStatusName(std::int32_t status_code, char* buffer, std::int32_t capacity);
    static int GetStatusCount();
    static int GetStatusCodeAt(std::int32_t index);
    static int GetStatusNameAt(std::int32_t index, char* buffer, std::int32_t capacity);
    static int GetSupportedFileSystemCount();
    static int GetSupportedFileSystemName(std::int32_t index, char* buffer, std::int32_t capacity);
    static int GetSupportedPlatformCount();
    static int GetSupportedPlatformName(std::int32_t index, char* buffer, std::int32_t capacity);
    static int GetSupportedImageFormatCount();
    static int GetSupportedImageFormatName(std::int32_t index, char* buffer, std::int32_t capacity);
    static int GetInvalidHandleValue();
    static int GetHandleLifecycleSummary(char* buffer, std::int32_t capacity);
    static int GetHandleValueSummary(char* buffer, std::int32_t capacity);
    static int GetBufferStringPolicySummary(char* buffer, std::int32_t capacity);
    static int GetMutationPolicySummary(char* buffer, std::int32_t capacity);
    static int GetExportCount();
    static int GetExportNameAt(std::int32_t index, char* buffer, std::int32_t capacity);
    static int GetExportGroupAt(std::int32_t index, char* buffer, std::int32_t capacity);
    static int GetMutatingOperationCount();
    static int GetMutatingOperationNameAt(std::int32_t index, char* buffer, std::int32_t capacity);
    static int GetOpenModeSummary(char* buffer, std::int32_t capacity);
    static int GetOpenModeCount();
    static int GetOpenModeNameAt(std::int32_t index, char* buffer, std::int32_t capacity);

    // Filesystem and Metadata
    static int GetFileSystemInfo(std::int32_t handle, void* info_ptr);
    static int GetContainerMetadata(std::int32_t handle, void* metadata_ptr);
    static int GetFilesCount(std::int32_t handle, std::int32_t* out_count);
    static int GetFiles(std::int32_t handle, void* buffer, std::int32_t capacity);
    static int ReadFile(std::int32_t handle, const char* name, void* buffer, std::int32_t capacity);
    static int ReadSector(std::int32_t handle, std::int32_t cylinder, std::int32_t head, std::int32_t sector, void* buffer, std::int32_t capacity);
    static int DeleteFile(std::int32_t handle, const char* name);
    static int WriteFile(std::int32_t handle, const char* name, const void* data, std::int32_t length, std::uint16_t attributes, std::uint16_t load_address, std::uint16_t execution_address);
    static int WriteSector(std::int32_t handle, std::int32_t cylinder, std::int32_t head, std::int32_t sector, const void* data, std::int32_t length);
    static int RenameFile(std::int32_t handle, const char* old_name, const char* new_name);
    static int UpdateAttributes(std::int32_t handle, const char* name, std::uint16_t attributes);
    static int ReadBootArea(std::int32_t handle, void* buffer, std::int32_t capacity);
    static int WriteBootArea(std::int32_t handle, const void* data, std::int32_t length);
    static int Format(std::int32_t handle);
    static int Save(std::int32_t handle);
};
}
