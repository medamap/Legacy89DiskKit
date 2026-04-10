# Legacy89DiskKit

Legacy89DiskKit is a retro disk-image toolkit for Japanese 8-bit and 16-bit computer formats from the 1980s and 1990s.

The current public product focus is:

- `Legacy89DiskKit.Cli`: standalone end-user CLI
- `Legacy89DiskKit.CSharp`: supported managed integration surface
- `Legacy89DiskKit.Native`: documented native bridge companion
- `Legacy89DiskKit.Wasm`: planned future line

## Current Scope

Current disk container support:

- `.d88`
- `.d77`
- raw `.2d`
- raw `.dsk`

Current CLI command groups:

- `list`
- `file`
- `disk`
- `host`
- `boot`
- `layout`
- `inject`

The CLI is the primary release artifact.

## Build

```bash
./scripts/build.sh
```

```powershell
pwsh ./scripts/build.ps1
```

The build scripts run the managed build and tests first. If `cmake` is available, they also build the C++ library, run native tests, and then run the managed-to-native validation pass. When the native toolchain is unavailable, the scripts print a clear skip message because C# is currently ahead of C++ integration.

## Installed Command

```bash
l89 --help
```

## Packaging

The canonical local CLI release path is:

```bash
./scripts/release-cli.sh 2.1.0
```

An optional PowerShell companion exists for Windows:

```powershell
pwsh ./scripts/release-cli.ps1 -Version 2.1.0
```

Native companion release automation:

```bash
./scripts/release-native.sh 2.1.0
```

## Install

On macOS or Linux, install a published CLI and create the public `l89` command:

```bash
./scripts/install.sh
```

If your current shell does not pick up the updated `PATH` automatically, run the printed `export PATH=...` line once.

To install from an existing published directory instead:

```bash
./scripts/install.sh --source ./publish/v2.1.0/linux-x64 --prefix ~/.local
```

On Windows, install the CLI into the current user profile and add it to `PATH`:

```powershell
./scripts/install.ps1
```

The PowerShell installer also updates the current PowerShell session `PATH` when needed.

To install from an existing published directory instead:

```powershell
./scripts/install.ps1 -SourcePath .\publish\v2.1.0\win-x64
```

To uninstall:

```bash
./scripts/uninstall.sh
```

```powershell
./scripts/uninstall.ps1
```

The installed command is always:

```text
l89
```

## Quick Examples

Show CLI help:

```bash
l89 --help
l89 --full-help
```

Create blank media:

```bash
l89 disk create ./workdisk --image-format d88 --disk-type 2d
```

Format an existing image explicitly:

```bash
l89 disk format ./workdisk.d88 --file-system hu-basic
```

Inspect a disk image:

```bash
l89 ./workdisk.d88
l89 disk inspector ./workdisk.d88 --detail full
```

Import or export one file:

```bash
l89 file import ./workdisk.d88 ./hello.txt --target-name HELLO.TXT
l89 file export ./workdisk.d88 HELLO.TXT ./hello-out.txt
```

Inspect or move raw sectors:

```bash
l89 disk sector export ./workdisk.d88 0 1 ./sector.bin
l89 disk sector import ./workdisk.d88 0 ./sector.bin --count 1
l89 disk dump ./workdisk.d88 cylinder0,side0,sector1 32
```

Show boot metadata:

```bash
l89 boot show ./workdisk.d88
```

Check for a newer release:

```bash
l89 --check-update
```

When building a Windows release on a Windows machine with WiX v4 installed, create the MSI with:

```powershell
./scripts/build-cli-msi.ps1 -Version 2.1.0
```

## Guides

- [Platform support status](Documents/guides/Platform_Support_Status.md)
- [Common use cases](Documents/guides/Common_Use_Cases.md)
- [Release process](Documents/governance/Release_Process.md)
- [Roadmap V2 handoff](Documents/governance/Agent_Handoff_Roadmap_V2.md)
- [Roadmap V2 migration plan](Documents/governance/Roadmap_V2.md)
- [C# integration guide](Documents/guides/CSharp_Integration_Guide.md)
- [Native integration guide](Documents/guides/Native_Integration_Guide.md)
- [WASM integration guide](Documents/guides/Wasm_Integration_Guide.md)

## Notes

- `--language` changes CLI UI language.
- `--encoding` overrides filename decoding and related text I/O encoding.
- Public release work is being reset toward a fresh `v2.1.0` line.
