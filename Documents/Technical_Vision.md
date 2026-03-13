# Legacy89DiskKit Technical Vision

## Mission

Preserve and extend Japanese retro disk knowledge with modern tooling, explicit interfaces, and portable implementation boundaries.

## Product Direction After v2.0.0

The project now has a defined product-line model:

- `Legacy89DiskKit.Cli`: standalone end-user tool
- `Legacy89DiskKit.CSharp`: supported managed integration surface and current reference implementation
- `Legacy89DiskKit.Native`: current bridge ABI over the managed implementation
- `Legacy89DiskKit.Wasm`: documented future runtime line with a path-independent API direction
- `Legacy89DiskKit.Cpp`: future portable core line

The key post-`v2.0.0` transition is not "add more features first". It is to separate:

- the current reference implementation
- the future portable implementation

## Why the C++ Core Matters

The long-term goal includes embedded and bare-metal-oriented targets. That makes a pure managed implementation insufficient as the final portability layer.

The intended transition is:

```text
Current state:
  C# reference implementation
  -> Native bridge ABI
  -> CLI and host tooling

Next state:
  C++ portable core
  -> Native ABI backed by C++
  -> C# binding and host integration
  -> WASM/runtime targets
  -> embedded and bare-metal exploration
```

This means:

- C# remains important as the behavioral reference
- Native remains useful as the short-term bridge contract
- C++ becomes the long-term portability anchor

## Reference Implementation vs Portable Core

The project should now use these terms consistently:

- **Reference implementation**
  - the current C# implementation
  - used to define expected behavior
  - still supported for host applications
- **Portable core**
  - the future `Legacy89DiskKit.Cpp`
  - intended to carry disk/container parsing, filesystem rules, encoding logic, and low-level reusable behavior
  - intended to support native, WASM, embedded, and eventually bare-metal-oriented targets

This distinction is required before deeper migration work starts.

## Phase 20 Technical Direction

The immediate work after `v2.0.0` should define the future C++ transition boundary.

The first tasks are:

1. define `Legacy89DiskKit.Cpp` as the future portable core line
2. define which current C# subsystems are the first portability candidates
3. move path-dependent and host-dependent concerns out of the future core boundary
4. prefer buffer-first and path-independent public contracts
5. define how the current `Legacy89DiskKit.Native` bridge ABI can later sit on top of the C++ core

The intended first-port candidates are:

- disk container core
- character encoding core
- filesystem parsing and write rules

The intended execution order is:

1. disk container core
2. character encoding core
3. filesystem parsing and write rules

This order keeps the lowest shared dependencies first and reduces the chance of redesigning filesystem logic before the lower-level portable rules are stable.

## First Implementation Slice

The first concrete implementation slice for `Legacy89DiskKit.Cpp` should be intentionally narrow.

The recommended initial slice is:

1. read-only disk container open
2. low-level geometry and sector access
3. stable in-memory image representation
4. no host path discovery inside the core contract

The purpose of this slice is to prove:

- buffer-based image opening
- deterministic container parsing behavior
- explicit error and result handling at the core boundary
- compatibility with future filesystem logic layered above it

This first slice should avoid taking on filesystem mutation, host filesystem I/O, or CLI-facing formatting concerns.

The preferred first concrete extraction targets from the current C# codebase are:

1. raw-disk geometry detection and sector-offset calculation
   - currently concentrated in `RawDiskContainer`
2. D88 header parsing and track-sector parsing
   - currently concentrated in `D88DiskContainer`
3. the minimal container-side metadata contract
   - `DiskType`
   - `SectorInfo`
   - the read-oriented portion of the current container contract

The preferred near-term refactoring strategy is:

- separate buffer-based parsing logic from file-path loading and saving
- keep read-only container behavior ahead of write-path reconstruction
- avoid treating the current `DiskService` path-based convenience flow as the future portable boundary

## Boundaries to Preserve

The future core should aim to keep:

- disk and container parsing
- filesystem detection and explicit selection logic
- file listing, read, and write rules
- layout core logic where it is not CLI-presentation-specific
- encoding conversion rules
- stable metadata and result models

The future core should avoid:

- local path I/O as a required interface
- CLI-specific presentation formatting
- host-specific release automation
- localization concerns
- managed bootstrap wiring as part of the core contract

The future core should also avoid treating these host-side concerns as core responsibilities:

- local file discovery
- command-line argument parsing
- terminal rendering
- release packaging rules
- repository-specific sample-path assumptions
- user-facing document and help generation

The target public shape for the future core is:

- buffer-first
- path-independent
- explicit about ownership
- explicit about status or result handling
- suitable for reuse from native, managed, and runtime-hosted environments

The preferred contract style is:

- accept in-memory buffers or caller-provided abstractions rather than mandatory local paths
- prefer result-bearing APIs over exception-heavy control flow at the portability boundary
- keep encoding choice explicit by logical encoder name
- keep serializable models stable enough for managed, native, and future WASM hosts

## Native Bridge Migration

`Legacy89DiskKit.Native` should be treated as the migration bridge, not as the final low-level implementation.

The intended path is:

1. keep the documented `ldk_*` ABI backed by the C# reference implementation
2. build `Legacy89DiskKit.Cpp` beneath that portability contract
3. move the native ABI backing implementation from C# to C++ where practical
4. preserve the public C ABI unless a future major-version change makes adjustment unavoidable

This keeps native consumers attached to a stable contract while allowing the internal implementation to change.

## Host-Side Responsibilities to Keep in Managed Layers

The following responsibilities should remain outside the future portable core unless later evidence proves otherwise:

- local file loading and saving by host path
- command-line option parsing
- CLI presentation, table formatting, and localized output
- release and packaging orchestration
- repository-specific sample lookup
- managed bootstrap convenience for host applications

The future core may support these workflows indirectly through caller-provided buffers and explicit adapters, but it should not require them as intrinsic responsibilities.

## CLI Transition Criteria

The CLI should not switch directly to a future C++ core at the first sign of a working prototype.

The intended sequence is:

1. keep the CLI on the managed `Application` layer while `Legacy89DiskKit.Cpp` reaches parity for the first-port subsystems
2. add managed bindings over the emerging C++ core
3. verify that the bound C++ path preserves the documented behavior for representative workflows
4. switch the CLI only after the C++-backed path is stable enough to replace the current managed implementation for the supported surface

The minimum transition gate should include:

- container open and basic geometry handling
- encoding conversion parity
- at least one filesystem family reaching practical parity for list, read, write, create, and format flows
- layout export and validation behavior that does not regress the documented managed surface
- native and managed smoke coverage using the same public contract assumptions

Until those conditions are satisfied, the CLI should remain a host application over the managed reference implementation.

## Embedded and Bare-Metal Direction

The project is explicitly interested in lower-level deployment targets, but those must remain downstream of the core transition work.

Recommended target order:

1. desktop and server native hosts
2. Linux-based embedded boards
3. browser and runtime-hosted environments
4. true bare-metal targets

Before true bare-metal work starts, the project should have:

- a path-independent core
- explicit ownership and ABI rules
- explicit encoding contracts
- a host-agnostic error model

## Disk Image API vs FDC-Facing API

The long-term architecture should not assume that every consumer wants direct filesystem-aware or image-container-aware access only.

Two distinct access surfaces are expected to matter in the future:

1. direct disk image access
   - open a disk image or in-memory image payload
   - inspect or modify container and filesystem data
   - read sectors and higher-level metadata directly
2. FDC-facing access
   - emulate controller-visible behavior
   - expose sector-oriented or later flux-oriented behavior through an FDC-style contract
   - let emulator integrations obtain data as if it had been delivered through a controller path rather than through a host-side convenience API

This distinction matters because many emulator integrations do not consume a host filesystem API. They interact with a floppy disk controller model and expect data through controller semantics.

## Future Raw Magnetic Stream Direction

The current project centers on sector-based disk image containers such as D88 and raw sector images.

In the longer term, the architecture should allow a lower-level raw magnetic-stream format where the stored payload represents controller-visible magnetic data rather than only decoded sectors.

That future raw direction may include:

- inter-sector gaps
- timing-sensitive layout details
- noise or intentionally malformed structures
- copy-protection-relevant physical behaviors
- data that is meaningful to an FDC path even when it is not cleanly representable as ordinary sectors

This is not a near-term implementation target, but it is an important architectural constraint:

- do not assume that every future disk source is just a clean side/cylinder/sector table
- do not define the future native or C++ core in a way that makes an FDC-facing access surface impossible
- keep room for a later controller-oriented data path beside the direct image-access path

The project should therefore evolve toward two compatible layers:

- a direct image/container/filesystem access layer
- a future FDC-facing runtime layer

For D88-backed workflows, the future FDC-facing layer may still serve data derived from sector images while presenting that data through a controller-oriented API. For true raw magnetic-stream sources, the same FDC-facing layer should later be able to expose controller-visible behavior without forcing the data into a purely sector-decoded abstraction first.

That is why the current managed/native bridge should not be treated as the final low-level solution.

## Long-Term Aim

The long-term aim is a toolkit family where:

- the CLI remains practical and accessible
- the managed integration surface remains productive
- the native ABI remains stable for consumers
- the C++ core becomes the portability anchor
- WASM and embedded targets grow from the same core assumptions

This is the route that best supports both practical host tooling and the project’s bare-metal ambitions.
