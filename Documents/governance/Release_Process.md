# Legacy89DiskKit Release Process

## Overview

This document describes the current release process for Legacy89DiskKit.

The `v2.0.0` release line is centered on one hard shipping requirement:

- `Legacy89DiskKit.Cli` must ship as a reliable standalone binary

The C# library remains supported and documented, but it is not the packaging gate for `v2.0.0`.
`Legacy89DiskKit.Native` is a companion deliverable for `v2.0.0`, with a documented C ABI and host-platform verification.
`Legacy89DiskKit.Wasm` is documented in `v2.0.0` as a browser-first, path-independent API direction, but it is not a release artifact.

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
- a documented `Legacy89DiskKit.Native` C ABI
- a public native header
- host-platform verified native companion artifact

### Not required to block `v2.0.0`

- a separate packaged C# library release channel
- fully verified multi-platform Native artifacts
- fully released WASM artifacts
- a WASM prototype project

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

## Native Companion Release Automation

The current source of truth for the native companion bridge release is:

```bash
./scripts/release-native.sh X.Y.Z
```

Example:

```bash
./scripts/release-native.sh 2.0.0
```

An optional Windows companion script exists:

```powershell
pwsh ./scripts/release-native.ps1 -Version X.Y.Z
```

The native release script:

1. validates the semantic version input
2. checks that `RELEASE_NOTES_vX.Y.Z.md` exists
3. publishes the host-platform native bridge artifact
4. stages the public header with the native library
5. runs the host smoke harness
6. creates a normalized native archive under `release/vX.Y.Z/`

The public product identity is `Legacy89DiskKit.Native`, even though the current implementation project remains `Legacy89DiskKit.NativeInterop`.

For `v2.0.0`, native verification is closed by documenting verified and unverified targets explicitly. Host-platform verification is mandatory. Broader target coverage is desirable but not a release blocker.

## Pre-Release Document Updates

Update the following before tagging:

- `README.md`
- `Documents/governance/ROADMAP.md`
- `Documents/governance/Roadmap_V2.md`
- `RELEASE_NOTES_vX.Y.Z.md`
- `Documents/guides/Wasm_Integration_Guide.md`

Requirements:

- README must describe the real CLI public surface
- README must describe the real native bridge identity and support policy
- README must describe WASM as documented-only for `v2.0.0`
- release process must match the actual project layout
- roadmap must reflect the current product model
- Roadmap V2 must remain the active migration backlog

## Release Notes

Create or refresh:

- `RELEASE_NOTES_vX.Y.Z.md`

For `v2.0.0`, the release note should explicitly explain:

- the CLI-first packaging model
- the standalone distribution goal
- the role of the C# library
- the documented Native bridge layer and its host-platform verification model
- the documented but non-shipping WASM line
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
  - set `LEGACY89_SAMPLE_IMAGE` to a local disk image path when you want a media-based smoke check
  - if `LEGACY89_SAMPLE_IMAGE` is not set, the release scripts still run help-only smoke checks

The native release script expects:

- native project:
  - `CSharp/Legacy89DiskKit.NativeInterop/Legacy89DiskKit.NativeInterop.csproj`
- smoke harness:
  - `CSharp/NativeInteropTestApp/NativeInteropTestApp.csproj`
- public header:
  - `include/legacy89diskkit_native.h`
- host smoke-test sample image:
  - set `LEGACY89_SAMPLE_IMAGE` to a local disk image path when you want a media-based smoke check
  - if `LEGACY89_SAMPLE_IMAGE` is not set, the release scripts still run help-only smoke checks

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
- `Legacy89DiskKit.Cli-vX.Y.Z-win-x64.msi` (when built on Windows with WiX v4)
- `Legacy89DiskKit.Native-vX.Y.Z-<host-rid>.zip|tar.gz`

## Smoke Checks

The local release script verifies at minimum:

- `Legacy89DiskKit.Cli --help`
- `Legacy89DiskKit.Cli disk --help`
- `Legacy89DiskKit.Cli list --help`
- one real command against a known sample image on the host platform

The installed-command smoke check should verify at minimum:

- `l89 --help`
- `l89 --check-update`

Also verify that documented options match the actual CLI:

- `--language/-l`
- `--encoding/-e`
- `--file-system/-f`
- `--disk-type/-d`
- `--name/-n`

For the native companion bridge, verify at minimum:

- the public header is packaged with the native artifact
- the host smoke harness can open a sample image
- filesystem info retrieval works
- file count retrieval works
- file enumeration works
- the disk handle closes cleanly

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

## Manual Native Publish Reference

The native companion uses NativeAOT shared-library publishing for the verified host platform.

The effective script behavior is equivalent to:

```bash
dotnet publish CSharp/Legacy89DiskKit.NativeInterop/Legacy89DiskKit.NativeInterop.csproj -c Release -r <host-rid> -p:PublishAot=true -p:NativeLib=Shared -o publish/vX.Y.Z/native/<host-rid>/build
```

## Optional Windows MSI Packaging

On a Windows host with WiX v4 installed, create the per-user MSI after the `win-x64` standalone publish is available:

```powershell
./scripts/build-cli-msi.ps1 -Version X.Y.Z
```

## Final v2.0.0 Closure Checklist

Complete these in order before tagging:

1. update `RELEASE_NOTES_v2.0.0.md`
2. run `./scripts/release-cli.sh 2.0.0`
3. run `./scripts/release-native.sh 2.0.0`
4. confirm CLI smoke checks passed
5. confirm native smoke checks passed
6. confirm README, release process, and integration guides are aligned
7. confirm `Documents/governance/Roadmap_V2.md` and `Documents/governance/ROADMAP.md` are aligned with the current migration and release state
8. only then create the tag and release

Mandatory `v2.0.0` deliverables:

- standalone CLI archives for the official CLI matrix
- documented C# public surface
- public native header and guide
- host-platform verified native companion artifact
- documented WASM API direction
- release scripts and final release notes

Not blockers for `v2.0.0`:

- a WASM prototype project
- full multi-platform native bridge verification
- first `Legacy89DiskKit.Cpp` implementation work
- bare-metal proof-of-concept targets

The staged package then exposes the library under the public product name `Legacy89DiskKit.Native.*` together with:

- `include/legacy89diskkit_native.h`

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
  release/vX.Y.Z/Legacy89DiskKit.Cli-vX.Y.Z-osx-arm64.tar.gz \
  <native-archive-path>
```

## `v2.0.0` Gate Checklist

Do not tag `v2.0.0` until all of these are true:

- local release automation is in place and verified
- CLI publishes as self-contained single-file for the official matrix
- native bridge ABI is documented and packaged with a public header
- host-platform native companion artifact is verified with the smoke harness
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
