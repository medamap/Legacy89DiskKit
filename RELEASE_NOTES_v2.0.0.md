# Legacy89DiskKit v2.0.0

## Summary

Legacy89DiskKit `v2.0.0` is the packaging and product-boundary release.

This release makes the CLI the primary end-user artifact and defines the public product lines used by the project going forward:

- `Legacy89DiskKit.Cli`
- `Legacy89DiskKit.CSharp`
- `Legacy89DiskKit.Native`
- `Legacy89DiskKit.Wasm`

## Highlights

### Standalone CLI Packaging

- standalone CLI release artifacts for the official supported platforms
- self-contained single-file publishing as the default release path
- CLI-first release model for end users

### Product Boundary Definition

- `Legacy89DiskKit.Cli` defined as the end-user command-line tool
- `Legacy89DiskKit.CSharp` defined as the supported managed integration surface
- `Legacy89DiskKit.Native` defined as the documented native bridge line over the current managed reference implementation
- `Legacy89DiskKit.Wasm` defined as a documented browser-first and WASI-capable API direction for future work

### Native Bridge Companion

- documented `ldk_*` C ABI with a public header
- host-platform verified native companion release flow
- explicit bridge-layer positioning for future `Legacy89DiskKit.Cpp` work

### WASM Direction

- documented-only `Legacy89DiskKit.Wasm` line for `v2.0.0`
- path-independent and buffer-first API direction
- browser-first runtime model with WASI-capable design where practical

### Current CLI Capabilities

- disk listing and metadata inspection
- file injection and file operations
- explicit disk creation and formatting
- boot metadata inspection
- Hu-BASIC directory layout export, validation, and apply workflows

## Release Matrix

- Windows x64
- Linux x64
- macOS x64
- macOS arm64

## Native Companion Verification

- host-platform native bridge artifact verified
- documented public header included in native release package
- broader native platform verification remains in progress

## Known Limits

- the native bridge is still implemented by the current managed/native-interop layer
- native verification is host-platform-first in `v2.0.0`
- additional native targets beyond the current host may remain unverified
- Native AOT is not the required release path for this version
- `Legacy89DiskKit.Wasm` is defined in documentation only and does not ship as a `v2.0.0` build artifact
- some advanced CLI editing features remain roadmap items

## Verification

- core tests passed
- CLI help and documented command surface were aligned
- release documentation was updated to the current project structure
- standalone CLI publish commands were verified for the official release path

## Documentation

- `README.md`
- `Documents/Release_Process.md`
- `Documents/ROADMAP.md`
- `Documents/Roadmap_V2.md`

## Full Changelog

Replace this section with the final compare link before publishing:

- `v1.6.0...v2.0.0`
