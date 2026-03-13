# Glossary

This glossary captures project-specific terminology that is easy to confuse during the `v2.x` and `Phase 20` transition.

## ABI

Application Binary Interface.

In this project, ABI usually refers to the externally visible native contract used by `Legacy89DiskKit.Native`, including function names, argument types, struct layout, and ownership expectations.

If the ABI changes incompatibly, existing native callers may no longer work without recompilation or code changes.

## API

Application Programming Interface.

In this project, API means the supported call surface exposed to a caller, such as the C# `Application` layer or the documented native `ldk_*` C ABI.

## Bare-Metal

An execution environment without a full host operating system layer such as desktop Windows, Linux, or macOS.

For this project, bare-metal is a long-term target and should remain downstream of the portable C++ core work.

## Buffer-First

A design style where data is passed as memory buffers, in-memory images, or similar abstractions rather than as mandatory local filesystem paths.

This is a preferred future-core boundary for `Legacy89DiskKit.Cpp`.

## Controller-Facing API

A future access surface shaped like a floppy disk controller interaction model rather than a filesystem convenience API.

Typical concepts include:

- command and status registers
- track, sector, and data registers
- drive and side selection
- IRQ and DRQ visible state
- explicit timing progression

## Direct Image Access

A surface that opens, inspects, converts, or edits disk images and filesystems directly.

This is distinct from the future controller-facing API.

## D88/D77-Style Sector-Container Family

A broad runtime category for sector-container image formats that share the same general role even if their file extensions differ.

For the current architecture direction, D88 and D77 are treated as the same broad adapter family rather than as unrelated media classes.

## FDC

Floppy Disk Controller.

In this project, FDC usually refers to the future controller-oriented runtime path used by emulator integration and, later, lower-level preservation or replay workflows.

## Fidelity

How closely a controller-facing or media-facing implementation reproduces historical device behavior.

This project currently separates:

- early architectural proof
- command subset support
- later fidelity-heavy investigation

## Host-Side

Behavior that belongs to the caller, application shell, CLI, or operating-system-specific integration rather than to the future portable core.

Examples include:

- local path discovery
- command-line parsing
- terminal rendering
- release automation
- localized help text

## Legacy 89 Storage

The current provisional family-name candidate for a future project-owned long-term raw preservation container.

The current provisional extension candidate is `.l89`.

This naming is not locked yet.

## Lossless

Preserving information without discarding meaningful input details.

In this project, the term matters when comparing sector-only formats with encoded-track or lower-level magnetic preservation formats.

## Managed Reference Implementation

The current C# implementation that acts as the behavioral reference during the transition to a future portable C++ core.

## Minimal Command Subset

The smallest useful set of controller-style commands and state transitions needed to prove the controller-facing architecture before full fidelity work exists.

## Native Bridge

The current `Legacy89DiskKit.Native` direction, where a documented C ABI sits on top of the managed C# implementation.

This is not the final portable core. It is a bridge layer.

## Parity

Equivalent observable behavior between two implementations.

In this project, parity usually means that the future C++ implementation should return the same practical results as the current C# reference implementation for the same workflow.

It does not mean error correction.

## Path-Independent

A contract that does not require direct host-local filesystem paths as part of its core behavior.

This is a key design goal for `Legacy89DiskKit.Cpp`, `Legacy89DiskKit.Wasm`, and future controller-facing work.

## Preservation

Long-term retention of disk content and behavior as historical material.

In this project, preservation may require more than decoded sector data.

## Raw Magnetic Stream

A future lower-level representation for controller-visible magnetic information that may preserve details not expressible as ordinary decoded sectors.

This may eventually include:

- gaps
- noise
- malformed structures
- timing-sensitive layouts
- protection-relevant behavior

## Read-Only First

An implementation-order decision.

It means the project first stabilizes:

- open
- parse
- inspect
- read

before broader write or reconstruction behavior.

It does not mean physical write-protect semantics.

## Replay

Using preserved data to reproduce behavior again later, especially in emulator-like or controller-facing contexts.

Replay and preservation goals overlap, but they are not always identical.

## Sector-Addressable Medium

A mounted-medium abstraction that can answer questions such as whether a sector exists and can return decoded sector payloads.

This is useful for direct image access and sector-based tooling workflows.

## Working Capture Representation

A temporary or intermediate representation used during acquisition, analysis, or conversion.

This is distinct from the project-owned long-term preservation container.
