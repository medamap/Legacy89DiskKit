# C# Integration Guide

## Overview

`Legacy89DiskKit.CSharp` is the supported managed integration surface for `v2.0.0`.

In the current repository structure, this means:

- the supported entrypoint is the `Legacy89DiskKit.Application` layer
- the supported feature set is based on the CLI-equivalent workflows already exercised by the project
- `Domain` models may be used as returned data, state holders, and work models
- direct use of `Infrastructure` remains possible for advanced experiments, but it is not part of the supported contract

This guide focuses on the supported managed path.

## Supported vs Unsupported

### Supported

- creating a preconfigured managed bootstrap through `Legacy89DiskKitApplication`
- opening, creating, and formatting disks
- detecting filesystems or selecting them explicitly
- listing files and reading filesystem info
- importing and exporting files
- exporting, validating, and applying directory layout plans
- storing and passing `Domain` models such as file entries, filesystem info, layout models, and metadata

### Unsupported

- directly constructing concrete filesystem implementations from `Infrastructure`
- relying on parser internals or concrete parser behavior as a compatibility contract
- relying on provider registration behavior outside the managed bootstrap
- treating concrete infrastructure types as a stable public API

## Supported Entry Surface

Use `Legacy89DiskKit.Application.Legacy89DiskKitApplication` as the bootstrap surface.

Preferred services:

- `DiskService`
- `FileTransferService`
- `DirectoryLayoutService`
- `ExplicitFileSystemResolver`

`DiskCloneService` is available through the same bootstrap path, but the primary managed surface is centered on the same workflows currently proven by the CLI.

## Open a Disk and Detect a Filesystem

```csharp
using Legacy89DiskKit.Application;

using var diskService = Legacy89DiskKitApplication.CreateDiskService();
diskService.OpenDisk("samples/X1Demo.d88");

var fileSystem = diskService.FileSystem
    ?? throw new InvalidOperationException("File system was not detected.");

var info = fileSystem.GetFileSystemInfo();
var files = fileSystem.GetFiles().ToList();
```

## Create and Format a Disk with an Explicit Filesystem

```csharp
using Legacy89DiskKit.Application;
using Legacy89DiskKit.Domain.DiskImage.Model;

using var diskService = Legacy89DiskKitApplication.CreateDiskService();
diskService.CreateDisk("images/workdisk.d88", DiskType.TwoD, "WORKDISK");

var resolver = Legacy89DiskKitApplication.CreateExplicitFileSystemResolver();
using var fileSystem = resolver.Create("hu-basic", diskService.OpenDisk("images/workdisk.d88", readOnly: false));

fileSystem.Format();
resolver.InitializeForDetection(fileSystem);
```

## Export and Import a File

```csharp
using Legacy89DiskKit.Application;

using var diskService = Legacy89DiskKitApplication.CreateDiskService();
diskService.OpenDisk("samples/X1Demo.d88");

var fileSystem = diskService.FileSystem
    ?? throw new InvalidOperationException("File system was not detected.");
var transfer = Legacy89DiskKitApplication.CreateFileTransferService(fileSystem.GetFileSystemInfo(), "sjis");

transfer.ExportFile(fileSystem, "Start up.Bas", "Start up.Bas.txt");
transfer.ImportFile(fileSystem, "README.txt", "README.DOC", isAscii: true);
```

## Export, Validate, and Apply a Layout Plan

```csharp
using Legacy89DiskKit.Application;

using var diskService = Legacy89DiskKitApplication.CreateDiskService();
diskService.OpenDisk("samples/LayoutDemo.2D", readOnly: false);

var fileSystem = diskService.FileSystem
    ?? throw new InvalidOperationException("File system was not detected.");
var layout = Legacy89DiskKitApplication.CreateDirectoryLayoutService();

var planText = layout.ExportPlan(fileSystem);
var validation = layout.ValidatePlan(fileSystem, planText);

if (!validation.IsValid)
{
    throw new InvalidOperationException("Layout plan validation failed.");
}

layout.ApplyPlan(fileSystem, planText);
```

## Domain Models as Work Models

`Domain` models are intentionally allowed as part of the managed workflow.

Typical examples:

- keep `FileEntry` objects in application state
- store `DiskFileSystemInfo` in a view model
- retain directory layout models during editing
- pass metadata objects across higher-level application workflows

This usage is supported because these models are part of the managed result and work surface.

## C# Reference Baseline for Future C++ Work

The current managed reference baseline is:

- disk/container operations through `DiskService`
- file transfer flows through `FileTransferService`
- layout flows through `DirectoryLayoutService`
- `Domain` models returned by those services

This does not start the C++ migration by itself. It only defines the current managed behavior that future `Legacy89DiskKit.Cpp` work should preserve.
