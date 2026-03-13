# WASM Integration Guide

## Overview

`Legacy89DiskKit.Wasm` is a planned product line for browser-facing and portable runtime scenarios.

For `v2.0.0`, this line is **documented-only**. No WebAssembly build artifact is required for the release gate.

The purpose of this phase is to define the API contract that a future WASM implementation should preserve. That future implementation may be backed first by the current C# reference behavior and later by `Legacy89DiskKit.Cpp`.

## v2.0.0 Status

For `v2.0.0`, `Legacy89DiskKit.Wasm` means:

- a documented browser-first runtime direction
- a WASI-capable API shape where practical
- no required prototype project
- no required browser package
- no required WASI package

This line is not a shipped artifact in `v2.0.0`.

## Intended Runtime Model

The intended runtime order is:

- browser-first
- WASI-capable where the same path-independent contract makes sense

This means the public shape should avoid local-path assumptions and host-specific filesystem behavior.

## Supported API Shape Direction

The intended WASM-facing surface should be built around:

- in-memory disk image bytes
- path-independent filesystem operations
- serializable result models
- explicit encoder names
- text or structured layout-plan data

Preferred input forms:

- `byte[]`
- `ReadOnlyMemory<byte>`
- `Stream`
- equivalent buffer abstractions

Preferred operation groups:

- open or inspect a disk image from memory
- detect or explicitly select a filesystem
- list files
- read file contents into caller-owned buffers
- write file contents from caller-provided buffers
- export, validate, and apply layout plans using text or structured models
- return metadata using DTO-style or serializable models derived from the current domain behavior

Preferred selector forms:

- filesystem names such as `hu-basic`, `n88-basic`, and `msx-dos`
- logical encoder names rather than host-path assumptions

Preferred error direction:

- result/status-oriented contracts at the portability boundary
- avoid depending on exception-heavy host behavior as the public WASM-facing shape

## Not Part of the WASM Surface

The following are intentionally outside the planned WASM line:

- local filesystem path I/O
- host-specific release scripts
- CLI presentation formatting
- console-oriented help and localization behavior
- Native AOT bridge assumptions
- direct dependence on infrastructure-specific parser internals

## Reusable Baseline from the Current C# Reference Implementation

The future WASM line should preserve the behavior currently defined by the managed reference implementation in these areas:

- disk and container parsing
- filesystem open and detect behavior
- file list, read, and write behavior
- layout export, validate, and apply core logic
- encoding conversion rules

These are the portable behavior targets. They do not imply that the current host-path-based service boundaries should be exposed unchanged.

## Relationship to Future C++ Work

`Legacy89DiskKit.Wasm` is not the final portability layer.

The intended long-term relationship is:

- the current C# implementation defines reference behavior
- the WASM contract defines a path-independent and buffer-first surface
- future `Legacy89DiskKit.Cpp` work is expected to preserve that contract from beneath

This is why `v2.0.0` treats the WASM line as an API-definition milestone rather than a release artifact milestone.
