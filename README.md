# Legacy89DiskKit

Legacy89DiskKit is a C# library and CLI for working with Japanese retro disk images and filesystem layouts from the 1980s and 1990s.

The project is moving toward a `v2.0.0` product model with four named lines:

- `Legacy89DiskKit.Cli`: standalone end-user tool
- `Legacy89DiskKit.CSharp`: reusable managed library
- `Legacy89DiskKit.Native`: documented native bridge companion
- `Legacy89DiskKit.Wasm`: planned browser/runtime line

Today, the CLI is the primary release-critical artifact. The C# library remains supported for integration work. The native bridge is a documented companion deliverable with host-platform verification. WASM is a documented future line for `v2.0.0`, not a shipped artifact.

For managed integration, the supported public surface is centered on `Legacy89DiskKit.Application`. `Domain` models may be used as result and work objects. Direct `Infrastructure` usage remains possible for advanced experimentation, but it is not part of the supported compatibility contract.

For native integration, the public bridge contract is the documented `ldk_*` C ABI under the `Legacy89DiskKit.Native` product identity. The current implementation is still backed by the managed/native-interop bridge and is not the final portable bare-metal core.

For WASM planning, the current `v2.0.0` contract is documented-only and browser-first, with a path-independent and buffer-first API direction. No WASM artifact is required for `v2.0.0`.

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
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false
```

## Run

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll --help
```

## Standalone CLI Packaging

`v2.0.0` is being defined around standalone CLI delivery. The intended release matrix is:

- Windows x64
- Linux x64
- macOS x64
- macOS arm64

The default release path is self-contained single-file publishing rather than Native AOT. Native AOT remains an optional future optimization path if it becomes stable on all target platforms.

The canonical local release path is:

```bash
./scripts/release-cli.sh 2.0.0
```

For a local self-contained single-file build on macOS arm64:

```bash
dotnet publish CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  /p:EnableCompressionInSingleFile=true \
  -o images/test/publish-cli
```

The published executable will be:

```bash
images/test/publish-cli/Legacy89DiskKit.Cli
```

This script runs tests, publishes the standalone CLI for the official matrix, performs host smoke checks, and creates normalized archives under `release/v2.0.0/`.

See [Documents/governance/Release_Process.md](Documents/governance/Release_Process.md) for the release checklist and archive conventions.

An optional PowerShell companion exists for Windows:

```powershell
pwsh ./scripts/release-cli.ps1 -Version 2.0.0
```

Native companion release automation:

```bash
./scripts/release-native.sh 2.0.0
```

The native bridge currently guarantees host-platform verification and a documented C ABI. Broader native platform support remains an intended direction, but may still be unverified on the current release host.

Current `v2.0.0` native verification status:

- verified: host platform
- attempted but not yet verified: additional same-OS targets may still fail on the current release host
- not required for the `v2.0.0` gate: full multi-platform native bridge verification

## Minimal Examples

### List files and disk summary

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  list images/disk_org/x1/X1turboIIIDemo.d88 -e sjis
```

### Inject a host file into a disk image

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  inject images/test_inject.2D ./README.md
```

### Export, validate, and apply a layout plan

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  layout export images/disk_org/x1/XPL3A.2D > plan.txt

cat plan.txt | dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  layout validate images/disk_org/x1/XPL3A.2D --stdin

cat plan.txt | dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  layout apply images/disk_org/x1/XPL3A.2D --stdin
```

### Show boot metadata

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  boot show images/disk_org/x1/X1turboIIIDemo.d88
```

### Create and initialize a new disk image

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  disk create images/workdisk.d88 -d 2d -f hu-basic -n WORKDISK
```

### Create blank media without formatting

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk create images/test/BLANK_2D.D88 --disk-type 2d
```

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk create images/test/BLANK_2DD.D88 --disk-type 2dd
```

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk create images/test/BLANK_2HD.D88 --disk-type 2hd
```

### Reinitialize an existing image with an explicit filesystem

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  disk format images/workdisk.d88 -f hu-basic
```

`disk create` is the standard blank-media flow. `disk format` reinitializes an existing image and still supports detection-based formatting when no explicit filesystem is provided.

### Create an X-DOS destination disk

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk create images/test/XDOS_CLONE_2D.D88 --disk-type 2d --file-system xdos
```

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk create images/test/XDOS_CLONE_2DD.D88 --disk-type 2dd --file-system xdos
```

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk create images/test/XDOS_CLONE_2HD.D88 --disk-type 2hd --file-system xdos
```

### Copy only the boot area

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk boot-copy images/disk_org/x1/XDOS_SYS.D88 images/test/XDOS_CLONE_2D.D88 --force
```

### Copy all files one by one

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  file cross-copy images/disk_org/x1/XDOS_SYS.D88 images/test/XDOS_CLONE_2D.D88 all
```

The same pattern applies to `2dd` and `2hd`.

### X-DOS full system-clone example

```bash
images/test/publish-cli/Legacy89DiskKit.Cli \
  disk create images/test/XDOS_CLONE_2D.D88 --disk-type 2d --file-system xdos

images/test/publish-cli/Legacy89DiskKit.Cli \
  disk boot-copy images/disk_org/x1/XDOS_SYS.D88 images/test/XDOS_CLONE_2D.D88 --force

images/test/publish-cli/Legacy89DiskKit.Cli \
  file cross-copy images/disk_org/x1/XDOS_SYS.D88 images/test/XDOS_CLONE_2D.D88 all
```

## CLI Notes

- `--language` / `-l` changes UI language.
- `--encoding` / `-e` overrides disk filename decoding and related text I/O encoding.
- `disk create` options:
  - `--disk-type` / `-d`: required, `2d | 2dd | 2hd`
  - `--file-system` / `-f`: optional, `hu-basic | n88-basic | msx-dos | xdos`
  - `--name` / `-n`: optional disk label for supported containers
- `disk boot-copy` options:
  - `<src-image>`: source image path
  - `<dest-image>`: destination image path
  - `--force` / `-f`: skip overwrite confirmation
- `file cross-copy` arguments:
  - `<src-image>`: source image path
  - `<dest-image>`: destination image path
  - `<files>`: comma-separated filenames or `all`
- `layout export` writes to standard output unless `--output` is specified.
- `layout validate` and `layout apply` can read from standard input with `--stdin`.
- Hu-BASIC writes are currently limited to `65535` bytes per file. Files above that size are rejected before writing.

## Living Documents

- [Release process](Documents/governance/Release_Process.md)
- [Roadmap V2 handoff](Documents/governance/Agent_Handoff_Roadmap_V2.md)
- [Roadmap V2 migration plan](Documents/governance/Roadmap_V2.md)
- [C# integration guide](Documents/guides/CSharp_Integration_Guide.md)
- [Native integration guide](Documents/guides/Native_Integration_Guide.md)
- [WASM integration guide](Documents/guides/Wasm_Integration_Guide.md)
- [Roadmap](Documents/governance/ROADMAP.md)
- [Hu-BASIC disk format specification](Documents/platform/x1/HuBasic_Format_Specification.md)
- [D88 format reference](Documents/systems/common/D88_Format.md)
- [Document index](Documents/Folder.md)

## Notes

- Historical AI consultation notes and superseded planning documents were quarantined under `Documents/obsolete/2026-03-doc-audit/`.
- Deferred migration work remains tracked in `Documents/governance/Roadmap_V2.md`.
- GitHub Actions remains deferred. Local release automation is the current source of truth for standalone CLI packaging.
