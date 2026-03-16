#pragma once

#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include <cstdint>
#include <string>

namespace legacy89diskkit::cpp
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
};
}
