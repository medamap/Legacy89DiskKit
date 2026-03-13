# Native Integration Guide

## Overview

`Legacy89DiskKit.Native` is the public native bridge line for `v2.0.0`.

In the current implementation, it is backed by the managed/native interop project currently named `Legacy89DiskKit.NativeInterop`. That internal implementation name is not the public product contract.

The supported native contract is the documented `ldk_*` C ABI and the public header:

- `include/legacy89diskkit_native.h`

This native line is a bridge over the current C# reference implementation. It is not the final portable bare-metal core. Future `Legacy89DiskKit.Cpp` work is expected to preserve or replace this ABI from beneath.

## Supported vs Unsupported

### Supported

- opening and closing a disk handle
- creating a disk
- retrieving filesystem info
- counting and enumerating files
- reading and writing files
- deleting and renaming files
- updating file attributes
- reading and writing boot area data
- formatting a filesystem

### Unsupported

- direct access to infrastructure or parser internals
- reliance on internal managed project names or paths as a public contract
- assumptions about undocumented struct fields or buffer ownership
- treating the current bridge implementation as the final bare-metal solution

## ABI Contract

The public contract uses:

- UTF-8 input strings
- integer status codes
- handle-based lifecycle
- fixed-size output structs
- caller-owned buffers for read operations

General rules:

- input paths and file names are UTF-8 strings
- `ldk_open_disk` and `ldk_create_disk` return a positive handle on success
- error conditions are returned as negative `LdkStatus` values
- `ldk_close_disk` must be called for successful handles
- output structs must be provided by the caller
- output buffers must be allocated by the caller
- read functions return a byte count on success or a negative status code on failure

## Public Header

Use:

- `include/legacy89diskkit_native.h`

It defines:

- status codes
- disk type enum
- filesystem info struct
- file entry struct
- the supported `ldk_*` functions

## Host Verification Status for v2.0.0

For `v2.0.0`, the public native contract is documented and packaged as a companion deliverable.

Current verification status:

- host-platform native artifact verification is required and currently verified
- additional same-OS cross-arch verification may be attempted where practical
- broader native platform support remains an intended direction, but may not be fully verified on every release host

Current `v2.0.0` expectation:

- verified: current release host
- unverified but intended: additional native targets outside the current host verification path
- not a `v2.0.0` blocker: full multi-platform native bridge verification

The current native line should therefore be treated as:

- a documented and usable bridge API
- a companion deliverable for advanced consumers
- not yet the final portability layer for embedded or bare-metal deployment

## Example Usage from C

```c
#include "legacy89diskkit_native.h"
#include <stdio.h>

int main(void) {
    int32_t handle = ldk_open_disk("images/disk_org/x1/X1turboIIIDemo.d88", true);
    if (handle <= 0) {
        fprintf(stderr, "open failed: %d\n", handle);
        return 1;
    }

    LdkFileSystemInfo info;
    if (ldk_get_file_system_info(handle, &info) == LDK_STATUS_SUCCESS) {
        printf("filesystem: %s\n", info.file_system_name);
        printf("platform: %s\n", info.platform_id);
    }

    int32_t count = 0;
    if (ldk_get_files_count(handle, &count) == LDK_STATUS_SUCCESS) {
        printf("files: %d\n", count);
    }

    ldk_close_disk(handle);
    return 0;
}
```

## Relationship to Future C++ Work

`Legacy89DiskKit.Native` is the current bridge layer.

It exists so native consumers can target a stable documented ABI now, while the project continues to evolve toward a future `Legacy89DiskKit.Cpp` core.

The long-term intent is:

- current managed implementation defines behavior
- the bridge ABI gives native consumers a stable contract
- future `Legacy89DiskKit.Cpp` work preserves or replaces the implementation behind that contract

That is why consumers should target the documented C ABI, not internal managed details.
