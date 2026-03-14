# Legacy89DiskKit Roadmap

## Vision

Legacy89DiskKit is evolving from a C# retro disk library into a product family with clearly separated deliverables:

- a standalone CLI for end users
- a C# library for desktop and server integrations
- a native library for lower-level or embedded use
- a WebAssembly target for browser and portable runtime scenarios
- a future C++ core for low-level portability and bare-metal-oriented targets

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
- the C# library remains a supported reusable development target and reference implementation
- native and WASM deliverables have defined scope, even if they are still partial
- the future C++ core direction is documented as the long-term path toward embedded and bare-metal deployment
- the release process, README, and roadmap all describe the same product shape

## v2.0.0 Required Work

The `v2.0.0` path should be executed as a sequential readiness checklist.

The intended order is:

1. stabilize the standalone CLI release path
2. define and document the supported C# library surface as the current reference implementation
3. formalize the native library boundary and shipping expectations as the bridge layer
4. define the WASM-facing API shape and scope
5. close the remaining packaging and documentation gates
6. prepare the post-`v2.0.0` transition toward a C++ core and bare-metal-oriented targets

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

The C# library should remain a supported integration target, but it also needs to be treated as the current reference implementation for future non-.NET ports.

Required:

- define the intended public role of `Legacy89DiskKit.CSharp`
- identify the preferred entrypoints for host applications
- reduce ambiguity between low-level internal layers and supported application-facing services
- document how a C# consumer should open disks, access file systems, transfer files, and perform layout operations
- decide whether a dedicated facade package or namespace is required before `v2.0.0`
- identify which subsystems are stable enough to serve as the future porting baseline for C++ and bare-metal work

Current intended direction:

- the supported managed surface is centered on `Legacy89DiskKit.Application`
- `Domain` models are allowed as public result and work types
- direct `Infrastructure` usage remains possible, but unsupported
- the current managed bootstrap should provide the same default wiring currently assembled by the CLI

### 3. Native Library Formalization

The repository already contains a native interop prototype, but it is not yet a clearly shipped product line. In the long term, this layer should be treated as the bridge between the C# reference implementation and a future independent C++ core.

Required:

- rename or frame the current native line as `Legacy89DiskKit.Native`
- define the supported ABI surface and lifecycle rules
- provide a public header and usage contract for external native callers
- define what platforms and artifact forms are expected for native consumers
- decide whether Native remains documented-only in `v2.0.0` or becomes a packaged companion artifact
- keep the API shape compatible with a future replacement of the C# implementation by a true C++ core

Current intended direction:

- the public product identity is `Legacy89DiskKit.Native`
- the current implementation project may remain `Legacy89DiskKit.NativeInterop` internally
- `v2.0.0` should ship a documented native bridge contract plus host-platform verified companion artifact
- broader native platform verification remains desirable, but does not replace the requirement for a documented stable C ABI

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

Current intended direction:

- `Legacy89DiskKit.Wasm` is documented-only in `v2.0.0`
- the API direction is browser-first with a WASI-capable design where practical
- the public shape should be path-independent and buffer-first
- no `v2.0.0` build artifact is required

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

### 6. Post-v2 Core Transition

After `v2.0.0`, the project should begin shifting from a C#-centered implementation to a C++-centered portability strategy.

Required follow-up direction:

- define `Legacy89DiskKit.Cpp` as the future portable core line
- treat the current C# implementation as the reference implementation
- treat the future C++ implementation as the final portable core
- identify which parts of the current C# implementation should be ported first
  - disk container core
  - filesystem core
  - character encoding core
- keep path-dependent CLI and host concerns outside the future core boundary
- move toward buffer-based and path-independent service contracts where practical
- use the C# implementation as the reference behavior during the transition

Immediate `Phase 20` execution order:

1. define the `Legacy89DiskKit.Cpp` product line and its role
2. separate "reference implementation" from "final portable implementation" in public documents
3. identify first-port candidates
4. define which host and path concerns must stay outside the future core boundary
5. define how `Legacy89DiskKit.Native` transitions from a C# bridge ABI to a future C++-backed ABI

Recommended first-port execution order:

1. disk container core
2. character encoding core
3. filesystem parsing and write rules

This order is preferred because:

- container behavior is the lowest shared dependency
- encoding rules are portable logic with limited host coupling
- filesystem logic depends on both container rules and encoding behavior

Recommended first implementation slice:

1. read-only disk container open
2. low-level geometry and sector access
3. stable in-memory image representation
4. explicit result handling at the core boundary

This slice should prove the portability boundary before filesystem mutation or host-path convenience is carried into the new core.

Preferred first concrete extraction targets from the current C# implementation:

1. raw-disk geometry detection and sector-offset logic now concentrated in `RawDiskContainer`
2. D88 header parsing and track-sector parsing now concentrated in `D88DiskContainer`
3. the minimal read-oriented container metadata contract built around `DiskType`, `SectorInfo`, and the read-focused portion of the current container interface

The preferred immediate refactoring direction is to split buffer-based parsing from file-path loading and saving, and to keep read-only behavior ahead of write-path reconstruction.

The current managed reference implementation now already proves this direction in practice:

- raw-disk geometry and sector-offset logic have been separated into pure helper modules
- D88 header and track-sector parsing have been separated from the container shell
- the supported `Application` surface can open images from in-memory buffers with explicit format selection
- raw sector-image and D88/D77-style sector-container implementations already expose a shared read-only container metadata shape
- the managed reference implementation now exposes a read-only parser-result shape for D88 images and a raw-image descriptor path using the same metadata family
- the managed reference implementation now also exposes logical encoding identifiers and profile resolution separate from CLI-only wiring
- the managed reference implementation has started extracting platform-specific encoding tables into reusable pure data, beginning with the X1 character map
- the managed reference implementation has started extracting Hu-BASIC directory-entry rules into a reusable raw-entry codec separate from `FileEntry` mapping
- the managed reference implementation now also exposes reusable Hu-BASIC FAT and cluster-chain helper rules separate from the filesystem shell
- the managed reference implementation now also exposes reusable Hu-BASIC read-payload rules for terminal-length handling, recorded-size trimming, and ASCII EOF extraction
- the managed reference implementation now also exposes reusable Hu-BASIC allocation helpers for reserved-cluster rules and 2HD holey-FAT scanning
- the managed reference implementation now also exposes reusable Hu-BASIC write-path helpers for ASCII EOF appending, cluster-count calculation, and terminal-flag generation
- the managed reference implementation now also exposes reusable Hu-BASIC naming and virtual-label rules separate from the filesystem shell
- the managed reference implementation now also exposes reusable Hu-BASIC write-transaction helpers for FAT-chain application and directory-entry generation
- both D88/D77-style sector-container media and raw sector-image media have concrete mounted-medium adapters
- mounted media can already be bound into a minimal controller-facing path
- the controller-facing path already includes a minimal command subset and timing-driven completion in the managed reference implementation

This means the managed reference implementation has now completed the intended first implementation slice for the future `Legacy89DiskKit.Cpp` core. The next step is no longer to define the slice, but to begin translating the extracted contracts and pure-rule modules into the portable implementation line.

That translation has now started. The repository contains an initial `Legacy89DiskKit.Cpp` portability prototype with:

- a standalone CMake-based build path
- a portable result/status contract
- raw-disk geometry detection and sector-offset logic
- a read-only D88 parser that emits the same metadata/result family as the managed reference implementation
- a first logical character-encoding profile resolver

This prototype is intentionally narrow. It is not yet the production portable core, but it proves that the extracted contracts can already be carried into a non-managed implementation line.

### Future Core Boundary

The future `Legacy89DiskKit.Cpp` core should keep:

- disk image container parsing and low-level geometry rules
- filesystem detection and explicit filesystem selection rules
- directory parsing
- file read and write rules
- FAT and allocation logic
- boot metadata parsing and write rules where they are filesystem-level rather than host-level
- encoding conversion rules
- layout validation and transformation core logic
- stable metadata and DTO-like result models

The future core should exclude:

- local file path discovery and path-based convenience APIs as required interfaces
- CLI presentation and formatting
- release automation
- host-specific dependency setup
- localization and user-facing help text
- managed bootstrap wiring
- repository-specific sample image handling

The future core should also leave these responsibilities to host layers:

- command-line parsing and option policy
- terminal and table rendering
- artifact packaging and release verification orchestration
- host-specific path discovery and sample-path shortcuts
- user-facing document/help generation

The future core should be allowed to serve these workflows only through caller-provided adapters rather than direct host assumptions.

The desired portability boundary should be:

- buffer-first
- path-independent
- explicit about ownership
- explicit about result or status handling
- independent of console or GUI concerns

The preferred future core API style is:

- buffer-oriented rather than path-mandatory
- explicit about logical encoder names
- compatible with serializable metadata/result models
- conservative about exception-heavy control flow at the portability boundary

### Native Migration Direction

`Legacy89DiskKit.Native` should evolve in two stages:

1. current state
   - documented bridge ABI backed by the C# reference implementation
2. future state
   - the same public ABI backed by `Legacy89DiskKit.Cpp`, or a compatibility-preserving replacement ABI if a strict carry-over proves impossible

Until the C++ core exists, native consumers should treat the current ABI as stable at the C boundary but not as proof of final internal implementation structure.

The preferred migration rule is:

- preserve the documented `ldk_*` contract where practical
- change internals first
- change the C ABI only if necessary and only at a future major-version boundary

### CLI Transition Gate

The CLI should remain on the managed `Application` surface until the future C++ path is strong enough to replace it without shrinking the supported public behavior.

The preferred transition sequence is:

1. establish parity for the first-port subsystems beneath a managed binding layer
2. validate representative workflows against the bound C++ path
3. switch the CLI only after those workflows are stable enough to become the default implementation path

The minimum gate for that switch should include:

- disk container open and geometry behavior
- encoding conversion parity
- at least one filesystem family with practical parity for list, read, write, create, and format flows
- layout export and validation behavior that preserves the current documented contract
- smoke coverage for both managed and native-facing paths

### 7. Bare-Metal and Embedded Direction

The long-term ambition includes board-level and bare-metal-oriented targets.

This is not a `v2.0.0` gate, but it should guide architecture choices now.

Target direction:

- desktop and server native hosts first
- Linux-based embedded boards such as Raspberry Pi next
- browser/WASM and portable runtime scenarios in parallel where useful
- true bare-metal or board-specific ports only after the C++ core is mature

Design guidance:

- prefer buffer-based APIs over local-path-only APIs
- keep encoding and filesystem logic isolated from OS concerns
- keep ownership, error codes, and ABI rules explicit
- avoid treating the current C# native interop layer as the final bare-metal solution

### 8. Direct Image Access and Future FDC-Facing Access

The long-term runtime model should distinguish between:

- direct image/container/filesystem access
- controller-oriented access for emulator integration

The direct image path remains important for tooling, filesystem operations, and disk inspection.

The controller-oriented path matters because emulator integrations often expect a floppy-controller-style interface rather than a host-side filesystem API. A future runtime surface should therefore be able to expose D88-backed data through an FDC-facing contract even when the underlying source remains sector-based.

The preferred direction for that future FDC-facing contract is a controller-style model with:

- command and status register behavior
- track, sector, and data register state
- drive and side selection state
- IRQ and DRQ style signaling
- controller-driven sequencing rather than direct filesystem convenience calls

That means emulator-oriented integration should be treated as a controller-facing runtime problem, not merely as another form of direct sector helper API.

The preferred architectural split for this direction is:

- `DiskImage`
  - image containers and lower-level media representation
- `FileSystem`
  - filesystem-aware interpretation and tooling workflows
- `Drive`
  - mounted-medium state and drive-visible properties
- `Fdc`
  - controller-visible command, status, transfer, and signaling behavior
- `Timing`
  - clock or scheduler abstractions needed by controller-facing sequencing

For the near term, `Timing` can begin as a smaller controller-oriented abstraction rather than as a broad standalone subsystem.

The preferred layer split is:

- `Application` for drive-mount and controller-facing services
- `Domain` for drive/FDC/timing state and contracts
- `Infrastructure` for D88-backed and future raw-backed medium adapters

The first concrete medium-adapter candidates should be:

- `D88Backed...` for the D88/D77-style sector-container family
- `RawDiskBacked...` for raw sector-image families such as `.2d`

These first concrete adapter families already exist in the managed reference implementation and now serve as the baseline shape for future `Legacy89DiskKit.Cpp` porting work.

The minimum future FDC-facing public contract should cover:

- controller reset
- register-oriented command and status access
- track/sector/data register access
- drive and side selection
- media-ready and write-protect style state
- IRQ and DRQ visibility
- explicit timing progression through a clock or scheduler abstraction

The early contract should stay transportable:

- no mandatory host path I/O
- suitable for both D88-backed and future raw-backed media
- shaped for emulator integration rather than filesystem convenience

### 9. Future Raw Magnetic Stream Support

The architecture should also leave room for a later raw magnetic-stream source format.

That future direction is expected to represent controller-visible magnetic data rather than only decoded sectors, and may eventually include:

- inter-sector gaps
- noise
- malformed or timing-sensitive structures
- physical copy-protection behaviors

This does not make raw magnetic-stream support a current release target. It does mean the future core should avoid assuming that every disk source is permanently reducible to a clean side/cylinder/sector table.

The preferred future direction is to distinguish two lower-level preservation tiers:

1. encoded track data
   - FM- and MFM-level track payloads
   - per-track storage with media and timing metadata
   - suitable for preserving unusual track organization and many non-standard physical layouts
2. lower-level sampled or timing-oriented raw signal data
   - reserved for cases where encoded-track preservation is still not enough
   - relevant to stronger controller-visible or protection-relevant behavior

The first tier should be considered before any lower-level signal capture work because it offers a better balance between fidelity and practical handling.

The architecture should also assume asymmetric conversion:

- sector-only sources may be importable into an encoded-track container
- converting back from encoded or signal-oriented sources into sector-only formats may lose information or become impossible for some inputs

The preferred long-term shape is:

- direct image access as one stable surface
- FDC-facing access as another stable surface
- raw magnetic-stream sources added later beneath the FDC-facing surface without forcing them through a purely sector-decoded model first

The long-term preservation workflow should also assume:

1. real hardware capture through an FDC-visible path
2. possible use of an intermediate raw capture representation during acquisition
3. later conversion into a project-owned long-term preservation container

That means the project should eventually define not only a runtime-facing raw surface, but also a preservation-oriented raw container owned by the project itself.

The project-owned raw preservation container direction now provisionally reserves the family name `Legacy 89 Storage` and the extension `.l89`, but the concrete file structure and final frozen identity should remain open until the capture, conversion, and replay requirements are better understood.

The provisional identity should freeze only after all of the following are fixed:

- the capture-ingestion workflow
- the encoded-track payload model
- the conversion semantics from sector-only and lower-level raw inputs
- the required metadata, integrity, and format-version fields
- at least one validated fixture corpus against the frozen identity

The embedded and bare-metal direction should remain downstream of `Phase 20`. Do not start board-specific or hardware-specific implementation work until the C++ core boundary and migration policy are decision-complete.

The first concrete integration target for this direction should be emulator-hosted rather than board-hosted. This keeps the feedback loop fast while preserving the same controller-shaped contract that later embedded and bare-metal work will need.

The preferred order is:

1. emulator-hosted integration
   - first an event-driven host adapter
   - then a second host adapter with a different emulator-side integration style
2. desktop and server native hosts
3. Linux-based embedded boards
4. WASM/runtime-hosted experiments where appropriate
5. true bare-metal or custom-board targets

That order is now fixed as a Phase 21 planning result, not just a rough preference.

The architectural goal is not a universal host adapter. The goal is a shared narrow controller/core contract that can support multiple thin host-specific adapters.

That means:

- the core contract should remain host-agnostic
- timing progression should be host-driven through explicit tick or callback-style advancement
- each emulator or hardware environment should get its own thin integration adapter
- host adapters should translate drive selection, side selection, mount state, IRQ/DRQ visibility, and timing advancement into the shared controller/core contract

The first concrete host-integration order is now fixed:

1. an event-driven emulator host adapter
2. an xmil-web-style host adapter

This order should remain read-only at first. The first proof target is emulator-facing controller integration with:

- mounted-medium binding
- register-shaped access
- explicit timing advancement
- visible busy, IRQ, and DRQ state
- D88-backed media first
- raw sector-image-backed media second

Write support and higher-fidelity controller behavior should remain outside the first proof target.

The first adapter should translate an event-driven host controller shape into:

- mounted-medium binding
- register access
- explicit timing advancement
- IRQ and DRQ visibility
- drive-ready and drive-selection state

Where direct linking would create an avoidable license or distribution problem, the first adapter path should prefer a process-separated or IPC-friendly bridge so emulator-specific glue can remain on the host side.

That bridge should remain generic enough that it is not reasonably described as a dedicated integration path for one emulator codebase only. The repository should own the portable contract and transport shape, while host-specific bridge implementations stay with the host when license boundaries make that the safer choice.

The second adapter should prove that the same shared contract can also fit a host with:

- global controller state
- port-style read and write entrypoints
- host-owned event objects

The remaining deployment rule is straightforward:

- emulator-hosted work proves the contract
- desktop/server native hosts prove process-separated and non-managed integration
- Linux-based embedded boards prove constrained-host deployment
- true bare-metal work starts only after the host boundary and portable-core contracts stop moving

Before bare-metal work begins, the project should treat these as mandatory:

- path-independent portable core behavior
- explicit ownership and ABI rules
- explicit logical encoding contracts
- host-agnostic status and error reporting

The project should also keep these responsibilities host-side only during the first embedded push:

- CLI and presentation logic
- release and packaging automation
- managed bootstrap convenience surfaces
- emulator-specific transport bindings
- high-level host workflow orchestration

## Recommended v2.0.0 Scope

The safest `v2.0.0` scope is:

- standalone CLI release for four platforms
- current C# library retained, documented, and treated as the reference implementation
- native library scope documented, with the current interop layer assessed against a formal shipping checklist
- WASM scope documented as an active next target, with a defined API direction but not required to ship in `v2.0.0`
- future C++ core and bare-metal direction documented, but not required to ship in `v2.0.0`

In other words:

- CLI packaging must ship
- C# library must remain usable and reference-worthy
- native/WASM must be defined
- C++/bare-metal direction must be explicit
- native/WASM/C++ do not all need production completeness on day one

## After v2.0.0

### v2.x: Packaging and Runtime Expansion

Primary candidates:

- native library cleanup and supported API surface definition
- WebAssembly build target and minimal browser/runtime integration
- execute the `Legacy89DiskKit.Cpp` portability plan from the documented `Phase 20` sequence
- better release automation
- richer CLI help and localization
- filesystem-specific attribute editing and boot editing

Version guidance:

- `v2.0.1` should be reserved for post-release fixes if needed
- the `Phase 20` transition work is a likely `v2.1.0` candidate rather than a patch release

### v3.x: Broader Runtime Reach

Possible directions:

- mature the C++ core into the primary portable implementation
- stronger native embedding story
- browser-first tooling
- small-footprint and embedded scenarios
- board-specific and bare-metal deployment experiments
- conversion workflows between image/container families

## Deferred but Important Items

These remain valuable, but they are not the best `v2.0.0` gate items:

- full boot read/write CLI coverage
- filesystem-specific attribute editing from CLI
- layout plan direct metadata editing
- external language packs
- broader cross-filesystem layout editing
- deeper real-image verification matrix expansion
- C++ core migration work beyond the first documented transition stage
- bare-metal-target-specific hardware adaptation work

These should stay in the handoff task list rather than driving the version boundary alone.

## Release Intent Summary

Use this simple rule:

- `v1.x`: feature growth during architectural transition
- `v2.0.0`: packaging model and deliverable structure become intentional and sequentially verifiable
- `v2.x+`: the implementation strategy starts bending toward a portable C++ core and eventual bare-metal viability

That is why `v2.0.0` is justified even if not every future target is fully complete on the same day.

Controller-fidelity work remains a separate track from this portability-first line. The narrow controller-facing contract is meant to expose controller-shaped information access now, while MB8877-oriented behavior research proceeds independently on a dedicated branch:

- `codex/mb8877-fidelity-research`

## Rough Phase 22+ Direction

The phases below are intentionally rough. They should be treated as a living roadmap that can stretch, split, merge, or change order as implementation evidence arrives.

Each phase states both:

- what work it is expected to do
- what becomes possible after it is complete

### Phase 22: Controller Fidelity Research

Work:

- perform datasheet-first MB8877 behavior research
- compare the current narrow controller-facing path against real controller expectations
- define the first fidelity-oriented command, status, and timing milestones
- keep implementation guidance separate from host-specific adapter code

This makes it possible to:

- move from a narrow controller-shaped API to a controller behavior model with defensible semantics
- implement restore, seek, read-sector, and related status transitions with stronger confidence
- decide what belongs in the first practical emulator-facing fidelity milestone

### Phase 23: External Host Exposure and Transport Shape

Work:

- define the externally exposed transport shape for emulator hosts
- keep static and dynamic library use as first-class options
- define process-separated or IPC-friendly request and response transport for hosts that should not link directly
- identify candidate transport bindings such as local sockets, named pipes, stdio-style process bridges, or browser-friendly message bridges
- keep the first concrete transport thin: a stdio-oriented runner over the line-delimited text session rather than a mandatory always-on daemon shape
- support both plain request/response exchange and notification-aware exchange for IRQ, DRQ, and timing-advance hints
- support path-based image open and close from external hosts that do not own in-process container instances
- support explicit buffer-payload image open for browser-friendly and socket-style bridges that cannot rely on local host paths
- expose the first shipped host-facing process entrypoint through the CLI as `host stdio`

This makes it possible to:

- expose the controller-facing contract without forcing one linking model
- support direct embedding where licenses and deployment allow it
- support process-separated integration where a host-side bridge is the safer choice
- carry the same host-facing protocol into desktop, embedded, and browser-connected scenarios
- let host-side bridges choose between polling-only integration and notification-aware integration without changing the core controller contract
- start a host bridge as a thin process without writing a dedicated daemon first
- let an external emulator-side bridge open disk images by path and drive the controller contract immediately
- let browser-connected bridges start from uploaded or transferred image bytes without inventing a different controller protocol

### Phase 24: First Real Emulator Integrations

Work:

- implement the first out-of-repository host bridge against a real event-driven emulator host
- implement the second out-of-repository host bridge against a more C-style global-state emulator host
- validate that both hosts can use the same mounted-medium binding and controller-facing protocol
- verify read-only D88-backed and raw sector-image-backed flows end to end

This makes it possible to:

- run Legacy89DiskKit-backed controller access from real emulator hosts rather than only from internal tests
- prove that the shared controller/core contract works across more than one host architecture
- validate the transport and host-adapter split against real integration friction

### Phase 25: Portable Native Surface Consolidation

Work:

- align the public native ABI with the evolving C++ core and controller-facing contract
- decide how static libraries, shared libraries, and host-facing protocol layers relate to one another
- keep `Legacy89DiskKit.Native` compatible with both direct embedding and transport-based integration
- tighten ownership, lifetime, and status rules across the native boundary

This makes it possible to:

- treat direct static or shared-library embedding as a stable option
- keep a clean boundary between in-process linking and out-of-process protocol use
- reduce dependency on the managed bridge over time

### Phase 26: C++ Filesystem Parity Expansion

Work:

- continue porting filesystem-core logic into `Legacy89DiskKit.Cpp`
- finish read-oriented parity first and extend toward write-oriented parity
- expand encoding-core parity where filesystem behavior depends on it
- validate parity against the C# reference implementation

This makes it possible to:

- use the C++ core for more than container parsing alone
- move the portable implementation closer to replacing managed core behavior in selected flows
- prepare later native, embedded, and browser-facing deliverables on the same core

### Phase 27: Raw Preservation Format Formalization

Work:

- turn provisional `Legacy 89 Storage` and `.l89` naming into a frozen format identity when the gate conditions are satisfied
- define encoded-track and lower-level raw preservation semantics more precisely
- define conversion behavior from sector-only and lower-level raw sources
- define metadata, integrity, and versioning rules for the preservation container

This makes it possible to:

- preserve controller-visible magnetic information in a project-owned long-term container
- convert between sector-oriented and preservation-oriented representations with explicit expectations
- support future replay, archival, and protection-sensitive workflows on a documented basis

### Phase 28: Embedded and Bare-Metal Proof

Work:

- execute the first embedded host proof after the deployment order and start conditions are satisfied
- keep the first target read-only
- validate controller-facing and direct-image access in a constrained environment
- decide when write support and higher-fidelity controller behavior are safe to introduce

This makes it possible to:

- prove that the portable core can live outside desktop and emulator environments
- move from emulator-hosted experimentation to real constrained deployment
- start a true bare-metal or custom-board track with evidence instead of assumption
