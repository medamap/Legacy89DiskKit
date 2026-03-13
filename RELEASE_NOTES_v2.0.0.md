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
- `Legacy89DiskKit.Native` defined as a planned native-library line
- `Legacy89DiskKit.Wasm` defined as a planned browser/runtime line

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

## Known Limits

- Native and WASM lines are defined but not yet full release artifacts
- Native AOT is not the required release path for this version
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
- `Documents/handoff/task.md`

## Full Changelog

Replace this section with the final compare link before publishing:

- `v1.6.0...v2.0.0`
