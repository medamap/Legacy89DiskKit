# Legacy89DiskKit

Legacy89DiskKit is a retro disk-image toolkit for Japanese 8-bit and 16-bit computer formats from the 1980s and 1990s.

The current public product focus is:

- `Legacy89DiskKit.Cli`: standalone end-user CLI
- `Legacy89DiskKit.Application`: supported managed integration surface
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
dotnet build CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false
```

## Run

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll --help
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

## Quick Examples

Show CLI help:

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll --help
```

Create blank media:

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  disk create ./workdisk.d88 --disk-type 2d
```

Format an existing image explicitly:

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  disk format ./workdisk.d88 --file-system hu-basic
```

Show boot metadata:

```bash
dotnet CSharp/Legacy89DiskKit.Cli/bin/Debug/net9.0/Legacy89DiskKit.Cli.dll \
  boot show ./workdisk.d88
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
