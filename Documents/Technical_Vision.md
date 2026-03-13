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

The current managed reference implementation already demonstrates the first part of this portability boundary:

- buffer-based disk opening through the supported `Application` surface
- extracted raw-disk geometry and sector-offset logic
- extracted D88 header and track-sector parsing
- a shared read-only container metadata contract spanning raw sector-image and D88/D77-style sector-container implementations
- a portable read-only parser-result shape for D88 images plus a raw-image descriptor path for the same metadata family
- a logical character-encoding identity and profile resolution path above the concrete managed encoders
- the first platform-specific mapping table extraction by moving the X1 character map into reusable pure data
- the first Hu-BASIC filesystem-core extraction by splitting raw 32-byte directory-entry decoding from `FileEntry` mapping
- extracted Hu-BASIC FAT entry, cluster-chain, and terminal-flag interpretation rules into reusable pure helpers
- extracted Hu-BASIC read-payload trimming rules for terminal-length handling, recorded-size trimming, and ASCII EOF handling
- extracted Hu-BASIC allocation rules for reserved-cluster handling and 2HD holey-FAT scanning
- extracted Hu-BASIC write-payload rules for ASCII EOF appending, cluster-count calculation, and terminal-flag generation
- extracted Hu-BASIC file-name truncation and virtual-label detection/merge rules into reusable helpers
- extracted Hu-BASIC write-transaction rules for FAT-chain application and directory-entry generation
- concrete mounted-medium adapters for both D88/D77-style sector-container and raw sector-image families
- a minimal mounted-medium to controller-facing binding path for future FDC-oriented workflows

Taken together, these pieces now complete the managed reference version of the first implementation slice:

- buffer-first read-only container opening
- stable in-memory image representation
- shared read-only metadata and parser-result contracts
- logical encoding identity and extracted mapping data
- the first Hu-BASIC filesystem-core rules split into reusable helpers
- direct-image and narrow controller-facing access paths over the same mounted media

The repository now also contains an initial executable `Legacy89DiskKit.Cpp` prototype. That prototype currently proves:

- portable result/status handling without exception-heavy boundaries
- raw sector-image geometry detection and sector-offset logic
- read-only D88 header and sector parsing
- shared metadata and read-only parser-result shapes
- logical character-encoding profile resolution

This is still an early portability prototype rather than the final production core, but it is enough to establish that the first extracted contracts can already live outside the managed implementation.

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

The first practical integration target after the portable core boundary should be an emulator host, not a custom board. That gives the project a visible and debuggable proving ground for the controller-facing contract before lower-level deployment work begins.

The intended host-integration shape is:

- one shared narrow controller/core contract
- one thin adapter per host environment
- no universal host adapter assumption

In practice this means:

- event-driven emulator hosts should be able to drive the core through delayed callbacks or scheduled advancement
- step-driven hosts should be able to drive the same core through explicit ticking
- host adapters should translate mount state, selected drive, selected side, IRQ/DRQ visibility, and timing progression into the common controller-facing contract

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

The future FDC-facing surface should therefore be designed around a classic floppy-controller interaction model rather than around convenience filesystem calls.

The expected contract shape is closer to:

- command and status register behavior
- track, sector, and data register state
- drive and side selection state
- IRQ and DRQ style signaling semantics
- controller-driven read and write sequencing

This does not require a chip-perfect implementation in the earliest phase, but it does mean the architectural direction should remain compatible with a controller model of that class.

## Minimum FDC-Facing Public Contract

The future controller-facing API should start from a minimal, transportable contract rather than from a full emulator-specific implementation.

The minimum contract should be able to represent:

- controller reset
- register-oriented command submission
- register-oriented status reads
- track, sector, and data register reads and writes
- drive selection and side selection
- media-ready and write-protect style state
- IRQ and DRQ visible outputs
- stepwise progression driven by an explicit timing or clock abstraction

The preferred early contract style is:

- command/status/data abstractions first
- event or poll-friendly IRQ/DRQ visibility
- explicit drive attachment or mounting
- no mandatory dependency on a host filesystem path
- compatibility with both D88-backed media and future lower-level raw media

The earliest controller-facing API does not need to model every historical chip quirk. It does need to preserve the architectural shape of a controller-driven interaction model so that emulator integrations do not depend on direct filesystem convenience calls.

The managed reference implementation now already proves that shape in a deliberately narrow form:

- mounted media can be bound into drive-aware controller access
- sector-backed media can expose a minimal controller-style command subset
- visible controller state can be observed without consuming transfer data
- transfer completion can be driven through explicit timing progression rather than through an always-immediate path

## Additional Domain Boundaries for the FDC Direction

The controller-oriented direction should not be treated as merely an extension of the existing filesystem domain.

The future architecture should introduce additional domain concerns beside `DiskImage` and `FileSystem`:

- `Drive`
  - mounted-medium state
  - selected side and track position as drive-visible state
  - ready, motor, and media-presence style properties
- `Fdc`
  - controller command and status behavior
  - data-register and sector-transfer sequencing
  - IRQ/DRQ-oriented state transitions
- `Timing`
  - clock or scheduler abstractions required for controller-visible sequencing

For the near term, `Timing` does not need to become a large standalone domain. It may begin as a smaller clock or scheduler abstraction attached to the controller-oriented work and only later grow into a broader domain if that becomes necessary.

This means the future architecture should not force all emulator-facing behavior into `FileSystem`. The controller-facing model is a different concern and should be allowed to evolve as its own bounded context.

## Application and Infrastructure Responsibilities for the FDC Direction

The preferred layering for this future direction is:

- `Application`
  - expose host-facing services for drive mounting and FDC-oriented interaction
  - coordinate which mounted medium backs a given drive
  - expose controller-facing workflows without requiring the caller to know container internals
- `Domain`
  - define drive-visible state, controller-visible state, and timing-oriented abstractions
- `Infrastructure`
  - adapt concrete media sources such as D88-backed sector images or future raw magnetic-stream sources
  - provide the media-specific behavior needed by the controller-facing layer

Under this model:

- a D88-backed mounted medium may satisfy the controller-facing contract through an emulated sector-oriented adapter
- a future raw magnetic-stream source may satisfy that same controller-facing contract more directly
- direct image access remains separate from the controller-oriented path even when both are backed by the same underlying image source
- the sector-container family should treat D88-style and D77-style payloads as one broad runtime category with different container naming rather than as unrelated media classes

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

The intended future magnetic-stream container should be capable of storing encoded track-level data rather than only decoded sector payloads.

The expected characteristics of that future format are:

- per-image header metadata for media and timing assumptions
- per-track payloads rather than filesystem-oriented records
- support for FM- and MFM-level encoded data
- room for drive-relevant properties such as rotational assumptions and transfer characteristics
- the ability to preserve structures that are lost when an image is reduced to ordinary sectors

This future format is expected to be useful for:

- special physical formats with unusual sector layout
- preservation of gap structure and non-standard track organization
- controller-oriented emulation paths
- later experimentation with protection-relevant physical behavior

The project should also assume that a future encoded-track container may be convertible from ordinary sector images in one direction, while reverse conversion back into sector-only formats may be lossy or impossible for some sources.

In addition, the long-term direction should leave room for an even lower level than encoded tracks: sampled or timing-oriented raw signal data captured from real drives. That layer is expected to matter only for cases where track-level encoding is still not enough to preserve controller-visible behavior.

The expected preservation workflow is also important:

1. capture controller-visible magnetic information from a real drive path
2. store that capture in an existing raw-oriented working format if needed
3. convert the captured result into a project-owned preservation format
4. use the project-owned format as the long-term archival and interchange target

This means the future raw direction is not only about runtime playback. It is also about preservation workflow design.

The project should therefore be prepared for a pipeline where:

- real-hardware capture is performed through an FDC-oriented path
- a temporary or intermediate raw representation may exist during acquisition
- the long-term project format is distinct from the temporary capture representation

For planning purposes, the project may reserve a future project-owned preservation container name and extension for this direction, but that should not be treated as a locked file-format specification until the capture and conversion requirements are better understood.

The provisional `Legacy 89 Storage` and `.l89` identity should freeze only after:

- the capture-ingestion workflow is fixed
- the encoded-track payload model is fixed
- the conversion semantics from sector-only and lower-level raw inputs are fixed
- the required metadata, integrity, and format-version fields are fixed
- at least one fixture corpus has been validated against the frozen identity

The project should therefore evolve toward two compatible layers:

- a direct image/container/filesystem access layer
- a future FDC-facing runtime layer

For D88-backed workflows, the future FDC-facing layer may still serve data derived from sector images while presenting that data through a controller-oriented API. For true raw magnetic-stream sources, the same FDC-facing layer should later be able to expose controller-visible behavior without forcing the data into a purely sector-decoded abstraction first.

The intended long-term relationship is:

- direct image APIs remain appropriate for tooling and filesystem workflows
- the FDC-facing API becomes the emulator-facing contract
- both D88-backed sources and future lower-level raw sources can sit behind that controller-oriented surface

The first concrete mounted-medium implementations should therefore be planned in pairs:

- `D88Backed...` adapters for the D88/D77-style sector-container family
- `RawDiskBacked...` adapters for raw sector-image families such as `.2d`

The managed reference implementation now already includes these first adapter families so that the controller-facing architecture can be exercised before the first `Legacy89DiskKit.Cpp` port begins.

That is why the current managed/native bridge should not be treated as the final low-level solution.

Controller-fidelity work remains separate from this portability-first line. The narrow controller-facing contract is intended to provide controller-shaped information access now, while deeper MB8877-oriented behavior research proceeds independently on:

- `codex/mb8877-fidelity-research`

## Long-Term Aim

The long-term aim is a toolkit family where:

- the CLI remains practical and accessible
- the managed integration surface remains productive
- the native ABI remains stable for consumers
- the C++ core becomes the portability anchor
- WASM and embedded targets grow from the same core assumptions

This is the route that best supports both practical host tooling and the project’s bare-metal ambitions.
