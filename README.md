# Legacy89DiskKit

Legacy89DiskKit is a C# library and CLI for working with Japanese retro disk images and filesystem layouts from the 1980s and 1990s.

The current CLI focuses on practical disk inspection and editing workflows for:

- Hu-BASIC disks used by Sharp X1 systems
- N88-BASIC disks used by PC-8801 systems
- MSX-DOS disks

## Current Scope

### Disk image containers

- `.d88`
- `.d77`
- raw `.2d`
- raw `.dsk`

### Current CLI command groups

- `list`
- `file`
- `disk`
- `boot`
- `layout`
- `inject`

This repository intentionally documents only the commands that exist in the current CLI.

## Build

```bash
dotnet build csharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test csharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false
```

## Run

```bash
dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll --help
```

## Minimal Examples

### List files and disk summary

```bash
dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  list images/disk_org/x1/X1turboIIIDemo.d88 -e sjis
```

### Inject a host file into a disk image

```bash
dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  inject images/test_inject.2D ./README.md
```

### Export, validate, and apply a layout plan

```bash
dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  layout export images/disk_org/x1/XPL3A.2D > plan.txt

cat plan.txt | dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  layout validate images/disk_org/x1/XPL3A.2D --stdin

cat plan.txt | dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  layout apply images/disk_org/x1/XPL3A.2D --stdin
```

### Show boot metadata

```bash
dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  boot show images/disk_org/x1/X1turboIIIDemo.d88
```

### Create and initialize a new disk image

```bash
dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  disk create images/workdisk.d88 -d 2d -f hu-basic -n WORKDISK
```

### Reinitialize an existing image with an explicit filesystem

```bash
dotnet csharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  disk format images/workdisk.d88 -f hu-basic
```

`disk create` is the standard blank-media flow. `disk format` reinitializes an existing image and still supports detection-based formatting when no explicit filesystem is provided.

## CLI Notes

- `--language` / `-l` changes UI language.
- `--encoding` / `-e` overrides disk filename decoding and related text I/O encoding.
- `layout export` writes to standard output unless `--output` is specified.
- `layout validate` and `layout apply` can read from standard input with `--stdin`.
- Hu-BASIC writes are currently limited to `65535` bytes per file. Files above that size are rejected before writing.

## Living Documents

- [Release process](Documents/Release_Process.md)
- [Current project task list](Documents/handoff/task.md)
- [Hu-BASIC disk format specification](Documents/HuBasic_Format_Specification.md)
- [D88 format reference](Documents/D88_Format.md)
- [Document index](Documents/Folder.md)

## Notes

- Historical AI consultation notes and superseded planning documents were quarantined under `Documents/obsolete/2026-03-doc-audit/`.
- Deferred implementation items remain tracked in `Documents/handoff/task.md`.
