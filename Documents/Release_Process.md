# Legacy89DiskKit Release Process

## Overview

This document describes the current release process for Legacy89DiskKit.

The `v2.0.0` release line is centered on one hard shipping requirement:

- `Legacy89DiskKit.Cli` must ship as a reliable standalone binary

The C# library remains supported and documented, but it is not the packaging gate for `v2.0.0`.
Native and WASM lines are defined roadmap targets, not mandatory `v2.0.0` release artifacts.

## Versioning

Use semantic versioning.

- major: packaging model or public product boundary changes
- minor: backward-compatible feature additions
- patch: fixes and small non-breaking refinements

Examples:

- `v1.6.0 -> v1.7.0`: feature growth during the transition period
- `v1.x -> v2.0.0`: standalone CLI-first packaging reset

## Release Branch Preparation

```bash
git checkout develop
git merge feature/your-work --no-ff

git checkout main
git merge develop --no-ff -m "Merge branch 'develop' for vX.Y.Z release"
```

## Release Artifacts

### Mandatory for `v2.0.0`

- standalone CLI binary packages for:
  - Windows x64
  - Linux x64
  - macOS x64
  - macOS arm64

### Not required to block `v2.0.0`

- a separate packaged C# library release channel
- fully released Native library artifacts
- fully released WASM artifacts

## CLI Publish Strategy

The default release strategy is:

- self-contained
- single-file
- current CLI project path:
  - `CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj`

Native AOT is optional and should only be used if it is verified to work reliably across all intended CLI platforms.

## Local Release Automation

The current source of truth for CLI releases is the local release script:

```bash
./scripts/release-cli.sh X.Y.Z
```

Example:

```bash
./scripts/release-cli.sh 2.0.0
```

The script performs all required release preparation steps in order:

1. validates the semantic version input
2. checks that `RELEASE_NOTES_vX.Y.Z.md` exists
3. runs the core test suite
4. publishes the standalone CLI for the official matrix
5. verifies the expected executables exist
6. runs host smoke checks
7. creates normalized archives under `release/vX.Y.Z/`

An optional Windows companion script exists:

```powershell
pwsh ./scripts/release-cli.ps1 -Version X.Y.Z
```

GitHub Actions is intentionally deferred. If CI/CD is added later, it should call or mirror the same local release flow.

## Pre-Release Document Updates

Update the following before tagging:

- `README.md`
- `Documents/ROADMAP.md`
- `Documents/handoff/task.md`
- `RELEASE_NOTES_vX.Y.Z.md`

Requirements:

- README must describe the real CLI public surface
- release process must match the actual project layout
- roadmap must reflect the current product model
- handoff task list must remain the execution backlog

## Release Notes

Create or refresh:

- `RELEASE_NOTES_vX.Y.Z.md`

For `v2.0.0`, the release note should explicitly explain:

- the CLI-first packaging model
- the standalone distribution goal
- the role of the C# library
- the planned but non-blocking Native and WASM lines
- major user-visible CLI capabilities and limits

## Script Inputs and Paths

The local release script expects:

- version without leading `v`
- CLI project:
  - `CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj`
- test project:
  - `CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj`
- release notes:
  - `RELEASE_NOTES_vX.Y.Z.md`
- host smoke-test sample image:
  - `images/disk_org/x1/X1turboIIIDemo.d88`

## Packaging Layout

Publish outputs are created under:

- `publish/vX.Y.Z/win-x64/`
- `publish/vX.Y.Z/linux-x64/`
- `publish/vX.Y.Z/osx-x64/`
- `publish/vX.Y.Z/osx-arm64/`

Archives are created under:

- `release/vX.Y.Z/`

Archive names:

- `Legacy89DiskKit.Cli-vX.Y.Z-win-x64.zip`
- `Legacy89DiskKit.Cli-vX.Y.Z-linux-x64.tar.gz`
- `Legacy89DiskKit.Cli-vX.Y.Z-osx-x64.tar.gz`
- `Legacy89DiskKit.Cli-vX.Y.Z-osx-arm64.tar.gz`

## Smoke Checks

The local release script verifies at minimum:

- `Legacy89DiskKit.Cli --help`
- `Legacy89DiskKit.Cli disk --help`
- `Legacy89DiskKit.Cli list --help`
- one real command against a known sample image on the host platform

Also verify that documented options match the actual CLI:

- `--language/-l`
- `--encoding/-e`
- `--file-system/-f`
- `--disk-type/-d`
- `--name/-n`

## Manual Publish Reference

The local release script uses self-contained single-file publishing with:

- `-c Release`
- `--self-contained true`
- `-p:PublishSingleFile=true`
- `-p:PublishAot=false`

Manual publish commands are no longer the primary documented path, but the effective script behavior is equivalent to:

```bash
dotnet publish CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishAot=false -o publish/vX.Y.Z/win-x64
dotnet publish CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishAot=false -o publish/vX.Y.Z/linux-x64
dotnet publish CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishAot=false -o publish/vX.Y.Z/osx-x64
dotnet publish CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishAot=false -o publish/vX.Y.Z/osx-arm64
```

## Tagging

Create an annotated tag:

```bash
git tag -a vX.Y.Z -m "Release vX.Y.Z"
```

Push main and the tag:

```bash
git push origin main
git push origin vX.Y.Z
```

## GitHub Release

GitHub Release creation is not automated in this phase.

After the local release script has produced and validated the archives, GitHub release steps remain manual:

Create the GitHub release:

```bash
gh release create vX.Y.Z \
  --title "Legacy89DiskKit vX.Y.Z" \
  --notes-file RELEASE_NOTES_vX.Y.Z.md
```

Upload artifacts:

```bash
gh release upload vX.Y.Z \
  release/vX.Y.Z/Legacy89DiskKit.Cli-vX.Y.Z-win-x64.zip \
  release/vX.Y.Z/Legacy89DiskKit.Cli-vX.Y.Z-linux-x64.tar.gz \
  release/vX.Y.Z/Legacy89DiskKit.Cli-vX.Y.Z-osx-x64.tar.gz \
  release/vX.Y.Z/Legacy89DiskKit.Cli-vX.Y.Z-osx-arm64.tar.gz
```

## `v2.0.0` Gate Checklist

Do not tag `v2.0.0` until all of these are true:

- local release automation is in place and verified
- CLI publishes as self-contained single-file for the official matrix
- release packaging commands are verified against the current project structure
- README matches the real CLI public surface
- roadmap and handoff task list both describe `v2.0.0` consistently
- core tests pass
- `RELEASE_NOTES_v2.0.0.md` exists and is reviewed
- CLI smoke checks pass on the host platform

## Troubleshooting

If the local release script fails during publish:

```bash
dotnet restore
dotnet clean
dotnet build -c Release CSharp/Legacy89DiskKit.Cli/Legacy89DiskKit.Cli.csproj
```

If GitHub CLI is missing:

```bash
brew install gh
gh auth login
```
