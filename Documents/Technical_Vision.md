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

The target public shape for the future core is:

- buffer-first
- path-independent
- explicit about ownership
- explicit about status or result handling
- suitable for reuse from native, managed, and runtime-hosted environments

## Native Bridge Migration

`Legacy89DiskKit.Native` should be treated as the migration bridge, not as the final low-level implementation.

The intended path is:

1. keep the documented `ldk_*` ABI backed by the C# reference implementation
2. build `Legacy89DiskKit.Cpp` beneath that portability contract
3. move the native ABI backing implementation from C# to C++ where practical
4. preserve the public C ABI unless a future major-version change makes adjustment unavoidable

This keeps native consumers attached to a stable contract while allowing the internal implementation to change.

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

That is why the current managed/native bridge should not be treated as the final low-level solution.

## Long-Term Aim

The long-term aim is a toolkit family where:

- the CLI remains practical and accessible
- the managed integration surface remains productive
- the native ABI remains stable for consumers
- the C++ core becomes the portability anchor
- WASM and embedded targets grow from the same core assumptions

This is the route that best supports both practical host tooling and the project’s bare-metal ambitions.
