# Codex Consultation: Phase 5 - Native Interop Layer

## Objective
Implement a flat C-style API for `Legacy89DiskKit` that can be exported via Native AOT to C/WASM. The API will use handle-based resource management (disks, file systems).

## Context
- **Target**: C++ applications and WASM runtimes (e.g., Wasm3/WAMR on bare metal).
- **Functionality**: Open/Create disk, List files, Read/Write files, Rename, Delete, Update attributes, Format, Read/Write Boot Area.
- **Data Models**: Return structs for `FileEntry` and `FileSystemInfo` including vintage metadata (Load/Exec addresses).

## Proposed API Design

### 1. Resource Management
```c
// Handles are opaque integers or pointers
typedef int32_t DiskHandle;

[UnmanagedCallersOnly(EntryPoint = "ldk_open_disk")]
public static DiskHandle OpenDisk([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

[UnmanagedCallersOnly(EntryPoint = "ldk_close_disk")]
public static void CloseDisk(DiskHandle handle);
```

### 2. File Operations
```c
struct NativeFileEntry {
    const char* FileName;
    const char* Extension;
    uint32_t Size;
    uint16_t LoadAddress;
    uint16_t ExecutionAddress;
    uint16_t Attributes;
};

[UnmanagedCallersOnly(EntryPoint = "ldk_get_files")]
public static int LdkGetFiles(DiskHandle handle, NativeFileEntry* buffer, int maxCount);
```

### 3. Data Transfer
- How to handle `byte[]` transfer efficiently?
- Return pointers? Buffers allocated by caller?

## Specific Questions for Codex
1. **Handle Mapping**: What is the best way to map `int32_t` handles to C# object instances in a thread-safe High-Performance way without heavy overhead? (Dictionary? `GCHandle`?)
2. **String Marshalling**: Since this is directed at `Native AOT`, should I use `IntPtr` and `Marshal.PtrToStringUTF8` or can I rely on `[MarshalAs(UnmanagedType.LPUTF8Str)]`?
3. **WASM Compatibility**: Are there specific constraints for Native AOT when targeting WASM/Browser or WASI (e.g., no P/Invoke, specific marshalling limits)?
4. **Error Handling**: How should I propagate C# exceptions to the C caller? (Return codes? `errno`-style global?)
5. **Memory Management**: For `GetFiles`, returning an array of structs requires careful memory management. Is it better to have the caller provide a buffer, or have C# return a pointer to an internally managed buffer?

## User's Core Intent
"Support broader interop via Native API. Ensure metadata like load/execution addresses are accessible."
