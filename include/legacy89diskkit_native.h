#ifndef LEGACY89DISKKIT_NATIVE_H
#define LEGACY89DISKKIT_NATIVE_H

#include <stdbool.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

#if defined(_WIN32)
#define LDK_CALL __cdecl
#else
#define LDK_CALL
#endif

typedef enum LdkStatus {
    LDK_STATUS_SUCCESS = 0,
    LDK_STATUS_ERROR_GENERIC = -1,
    LDK_STATUS_ERROR_INVALID_HANDLE = -2,
    LDK_STATUS_ERROR_INVALID_ARGUMENT = -3,
    LDK_STATUS_ERROR_FILE_NOT_FOUND = -4,
    LDK_STATUS_ERROR_DISK_FULL = -5,
    LDK_STATUS_ERROR_READ_ONLY = -6,
    LDK_STATUS_ERROR_NOT_IMPLEMENTED = -7,
    LDK_STATUS_ERROR_BUFFER_TOO_SMALL = -8
} LdkStatus;

typedef enum LdkDiskType {
    LDK_DISK_TYPE_2D = 0,
    LDK_DISK_TYPE_2DD = 1,
    LDK_DISK_TYPE_2HD = 2,
    LDK_DISK_TYPE_HARD_DISK = 3
} LdkDiskType;

typedef struct LdkFileEntry {
    char file_name[16];
    char extension[8];
    int32_t size;
    uint16_t load_address;
    uint16_t execution_address;
    uint16_t attributes;
} LdkFileEntry;

typedef struct LdkFileSystemInfo {
    char file_system_name[32];
    int64_t total_capacity;
    int64_t free_space;
    int32_t cluster_size;
    int32_t reserved_sectors;
    char platform_id[16];
} LdkFileSystemInfo;

int32_t LDK_CALL ldk_open_disk(const char* path, bool read_only);
int32_t LDK_CALL ldk_create_disk(const char* path, int32_t disk_type, const char* name);
int32_t LDK_CALL ldk_close_disk(int32_t handle);
int32_t LDK_CALL ldk_get_abi_version(void);
int32_t LDK_CALL ldk_get_capability_flags(void);
int32_t LDK_CALL ldk_get_capability_summary(char* buffer, int32_t capacity);
int32_t LDK_CALL ldk_get_status_name(int32_t status_code, char* buffer, int32_t capacity);
int32_t LDK_CALL ldk_is_handle_valid(int32_t handle);
int32_t LDK_CALL ldk_get_open_handle_count(void);
int32_t LDK_CALL ldk_get_file_system_info(int32_t handle, LdkFileSystemInfo* info);
int32_t LDK_CALL ldk_get_files_count(int32_t handle, int32_t* out_count);
int32_t LDK_CALL ldk_get_files(int32_t handle, LdkFileEntry* buffer, int32_t capacity);
int32_t LDK_CALL ldk_read_file(int32_t handle, const char* name, void* buffer, int32_t capacity);
int32_t LDK_CALL ldk_delete_file(int32_t handle, const char* name);
int32_t LDK_CALL ldk_write_file(int32_t handle, const char* name, const void* data, int32_t length, uint16_t attributes);
int32_t LDK_CALL ldk_rename_file(int32_t handle, const char* old_name, const char* new_name);
int32_t LDK_CALL ldk_update_attributes(int32_t handle, const char* name, uint16_t attributes);
int32_t LDK_CALL ldk_read_boot_area(int32_t handle, void* buffer, int32_t capacity);
int32_t LDK_CALL ldk_write_boot_area(int32_t handle, const void* data, int32_t length);
int32_t LDK_CALL ldk_format(int32_t handle);

#ifdef __cplusplus
}
#endif

#endif
