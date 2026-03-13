# Legacy89DiskKit Roadmap

## Vision

Legacy89DiskKit is evolving from a C# retro disk library into a product family with clearly separated deliverables:

- a standalone CLI for end users
- a C# library for desktop and server integrations
- a native library for lower-level or embedded use
- a WebAssembly target for browser and portable runtime scenarios

The next major milestone is not just feature growth. It is a packaging and product-definition reset.

## Current Position

The repository is currently in a post-`v1.6.0` transition state.

Completed and available today:

- C# domain/application/infrastructure implementation
- filesystem support centered on Hu-BASIC, N88-BASIC, MSX-DOS, FAT12, CP/M, and CDOS
- a modern CLI project with structured commands
- layout-aware listing and directory-order editing for Hu-BASIC
- explicit disk creation and formatting from the CLI

Still unstable or incomplete for release packaging:

- cross-platform standalone CLI publishing
- release automation for current project structure
- consistent public packaging boundaries between CLI, C# library, native library, and future WASM artifacts

## v2.0.0 Goal

`v2.0.0` should mark the point where Legacy89DiskKit has a stable public packaging model.

It should mean:

- the CLI is intentionally distributed as a standalone end-user binary
- the C# library remains a supported reusable development target
- native and WASM deliverables have defined scope, even if they are still partial
- the release process, README, and roadmap all describe the same product shape

## v2.0.0 Required Work

The `v2.0.0` path should be executed as a sequential readiness checklist.

The intended order is:

1. stabilize the standalone CLI release path
2. define and document the supported C# library surface
3. formalize the native library boundary and shipping expectations
4. define the WASM-facing API shape and scope
5. close the remaining packaging and documentation gates

`v2.0.0` is ready only when every required item in that sequence is complete.

### 1. CLI Packaging Reset

The CLI should become the first-class end-user deliverable.

Required:

- lock the official CLI release matrix
  - Windows x64
  - Linux x64
  - macOS x64
  - macOS arm64
- keep standalone execution without requiring `dotnet`
- lock the supported publish path
  - self-contained single-file is the current baseline
  - Native AOT remains optional until it is stable enough to replace or supplement the baseline
- keep local release automation as the current source of truth
- verify the release script and artifact layout for the official matrix

This is the highest-priority `v2.0.0` requirement.

### 2. C# Library Definition

The C# library should remain a supported integration target, but it needs a clearer public shape than it has today.

Required:

- define the intended public role of `Legacy89DiskKit.CSharp`
- identify the preferred entrypoints for host applications
- reduce ambiguity between low-level internal layers and supported application-facing services
- document how a C# consumer should open disks, access file systems, transfer files, and perform layout operations
- decide whether a dedicated facade package or namespace is required before `v2.0.0`

### 3. Native Library Formalization

The repository already contains a native interop prototype, but it is not yet a clearly shipped product line.

Required:

- rename or frame the current native line as `Legacy89DiskKit.Native`
- define the supported ABI surface and lifecycle rules
- provide a public header and usage contract for external native callers
- define what platforms and artifact forms are expected for native consumers
- decide whether Native remains documented-only in `v2.0.0` or becomes a packaged companion artifact

### 4. WASM Definition

WASM is still a roadmap line and currently lacks a dedicated implementation target.

Required:

- define the intended runtime model
  - browser-facing
  - WASI-facing
  - or both
- define the API shape that does not depend on local filesystem paths
- identify what shared application/domain logic can be reused without CLI assumptions
- decide whether `Legacy89DiskKit.Wasm` is documented-only in `v2.0.0` or requires a minimal prototype project

### 5. Release Pipeline and Documentation Closure

The packaging model and public story must stay consistent across the repository.

Required:

- keep README compact and release-facing
- keep `Documents/handoff/task.md` as the execution backlog
- keep this roadmap focused on product direction
- keep `Documents/Release_Process.md` aligned with the script-based flow
- define which deliverables are mandatory in `v2.0.0`
- define which deliverables remain roadmap targets only
- prepare final release notes and tagging criteria

## Recommended v2.0.0 Scope

The safest `v2.0.0` scope is:

- standalone CLI release for four platforms
- current C# library retained and documented
- native library scope documented, with the current interop layer assessed against a formal shipping checklist
- WASM scope documented as an active next target, with a defined API direction but not required to ship fully in `v2.0.0`

In other words:

- CLI packaging must ship
- C# library must remain usable
- native/WASM must be defined
- native/WASM do not both need full production completeness on day one

## After v2.0.0

### v2.x: Packaging and Runtime Expansion

Primary candidates:

- native library cleanup and supported API surface definition
- WebAssembly build target and minimal browser/runtime integration
- better release automation
- richer CLI help and localization
- filesystem-specific attribute editing and boot editing

### v3.x: Broader Runtime Reach

Possible directions:

- stronger native embedding story
- browser-first tooling
- small-footprint and embedded scenarios
- conversion workflows between image/container families

## Deferred but Important Items

These remain valuable, but they are not the best `v2.0.0` gate items:

- full boot read/write CLI coverage
- filesystem-specific attribute editing from CLI
- layout plan direct metadata editing
- external language packs
- broader cross-filesystem layout editing
- deeper real-image verification matrix expansion

These should stay in the handoff task list rather than driving the version boundary alone.

## Release Intent Summary

Use this simple rule:

- `v1.x`: feature growth during architectural transition
- `v2.0.0`: packaging model and deliverable structure become intentional and sequentially verifiable

That is why `v2.0.0` is justified even if not every future target is fully complete on the same day.
