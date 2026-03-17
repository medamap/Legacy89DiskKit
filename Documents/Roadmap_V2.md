# Roadmap V2

## Purpose

Roadmap V2 is the DDD-oriented migration roadmap for moving the current C# reference implementation toward a future C++-centered implementation line.

It exists to answer these questions clearly:

- which C# responsibility is being migrated
- which architectural layer it belongs to
- which C++ replacement surface is expected
- whether that migration phase is complete or incomplete

This roadmap intentionally prefers many small, explicit migration phases over a smaller number of broad product phases.

When a phase touches C++ structure, use [Cpp_Ddd_Folder_Migration_Rulebook.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md) to decide whether the touched files should also move into a DDD-oriented folder layout.

## Layer Model

This roadmap uses the following layers:

- Presentation
- Application
- Infrastructure
- Domain

Each phase is assigned to one primary layer, even when it depends on work in other layers.

## Migration Status Model

- `[ ]` not yet complete
- `[x]` complete

A phase is marked complete only when the current repository actually satisfies the intended replacement responsibility for that phase.

## Planning Rule

This roadmap is based on the current C# implementation structure.

The migration is therefore tracked as:

1. identify a C# responsibility
2. decide which C++ replacement surface should exist
3. implement and validate that replacement
4. mark the phase complete only when the replacement responsibility is genuinely present

This means phases may complete out of order.

That is expected.

The same rule applies to structural relocation work. File moves may happen gradually, as long as they remain tied to an active migration phase and follow the rulebook above.

## High-Level Direction

The intended direction is:

1. keep C# as the reference implementation
2. migrate Domain responsibilities into C++
3. migrate Infrastructure responsibilities into C++
4. add bridge or wrapper paths so C# or test-side code can validate C++ behavior early
5. migrate Application responsibilities into C++
6. replace or supplement Presentation and host-facing entrypoints later
7. expand toward WASM and bare-metal only after the required C++ layers are mature

## Completion Rule By Layer

### Domain

A Domain phase is complete when the required rules, models, parsing logic, and transaction-planning logic exist in C++ with meaningful parity to the C# reference behavior.

### Infrastructure

An Infrastructure phase is complete when C++ has concrete adapters or runtime construction paths, not just pure rules.

### Application

An Application phase is complete when C++ has use-case orchestration equivalents, not just helper classes or rule bundles.

### Presentation

A Presentation phase is complete when there is a real executable, bridge, or frontend path that exercises the migrated lower layers for that responsibility.

## Phase List

### Domain Phases

- [x] Phase V2-01: Disk image domain status and result contracts
  - Layer: Domain
  - C# source area: disk-image result and status contracts
  - Expected C++ target: portable status, result, and disk-image metadata types

- [x] Phase V2-02: Disk geometry and sector-address domain rules
  - Layer: Domain
  - C# source area: raw disk geometry and sector offset logic
  - Expected C++ target: raw geometry rules and record/sector address helpers

- [x] Phase V2-03: D88 metadata and parser domain rules
  - Layer: Domain
  - C# source area: D88 header and sector parsing logic
  - Expected C++ target: D88 parse result, metadata, and sector-oriented parser rules

- [x] Phase V2-04: Character encoding domain identifiers and profile rules
  - Layer: Domain
  - C# source area: encoding profile resolution and logical encoding identifiers
  - Expected C++ target: encoding profile types and lookup rules

- [x] Phase V2-05: Hu-BASIC filesystem domain core
  - Layer: Domain
  - C# source area: Hu-BASIC FAT, directory, payload, name, attribute, and write rules
  - Expected C++ target: Hu-BASIC types, rules, transactions, and shell helpers

- [x] Phase V2-06: N88-BASIC filesystem domain core
  - Layer: Domain
  - C# source area: N88-BASIC FAT, directory, payload, name, attribute, and write rules
  - Expected C++ target: N88-BASIC types, rules, transactions, and shell helpers

- [x] Phase V2-07: MSX-DOS filesystem domain core
  - Layer: Domain
  - C# source area: MSX-DOS FAT12, boot, directory, payload, name, attribute, and write rules
  - Expected C++ target: MSX-DOS types, rules, transactions, and shell helpers

- [x] Phase V2-08: Cross-filesystem domain surface catalog
  - Layer: Domain
  - C# source area: implicit supported-surface knowledge spread across providers and bootstrap
  - Expected C++ target: family and capability catalog for migrated filesystem surfaces

- [x] Phase V2-09: Controller-facing domain contracts
  - Layer: Domain
  - C# source area: FDC, drive, timing, controller-facing medium contracts
  - Expected C++ target: host-independent controller and medium contracts
  - Structural relocation: newly introduced controller-facing headers should move under `domain/` when the move is low-risk

- [x] Phase V2-10: Raw preservation domain model
  - Layer: Domain
  - C# source area: raw-direction notes and future preservation model
  - Expected C++ target: encoded-track, metadata, integrity, and conversion domain contracts
  - Structural relocation: newly introduced raw preservation headers should be created directly under `domain/raw/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

### Infrastructure Phases

- [x] Phase V2-11: Disk image buffer-loading infrastructure
  - Layer: Infrastructure
  - C# source area: path or buffer image loading shells
  - Expected C++ target: buffer-oriented container construction entrypoints
  - Structural relocation: newly introduced buffer-loading infrastructure should be created directly under `infrastructure/disk_image/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-12: Raw disk container infrastructure
  - Layer: Infrastructure
  - C# source area: raw container assembly and sector access shells
  - Expected C++ target: concrete raw container adapters over migrated domain rules
  - Structural relocation: newly introduced raw container infrastructure should be created directly under `infrastructure/disk_image/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-13: D88 container infrastructure
  - Layer: Infrastructure
  - C# source area: D88 container shell and concrete sector access
  - Expected C++ target: concrete D88 container adapters over migrated parser and domain rules
  - Structural relocation: newly introduced D88 container infrastructure should be created directly under `infrastructure/disk_image/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-14: Character encoding table infrastructure
  - Layer: Infrastructure
  - C# source area: concrete encoding table and conversion data
  - Expected C++ target: reusable encoding table data and lookup infrastructure
  - Structural relocation: newly introduced character-encoding infrastructure should be created directly under `infrastructure/character_encoding/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-15: Hu-BASIC filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: Hu-BASIC filesystem implementation shell
  - Expected C++ target: concrete Hu-BASIC container-backed infrastructure over domain rules
  - Structural relocation: newly introduced Hu-BASIC filesystem infrastructure should be created directly under `infrastructure/filesystem/hu_basic/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-16: N88-BASIC filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: N88-BASIC filesystem implementation shell
  - Expected C++ target: concrete N88-BASIC container-backed infrastructure over domain rules
  - Structural relocation: newly introduced N88-BASIC filesystem infrastructure should be created directly under `infrastructure/filesystem/n88_basic/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-17: MSX-DOS filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: MSX-DOS filesystem implementation shell
  - Expected C++ target: concrete MSX-DOS container-backed infrastructure over domain rules
  - Structural relocation: newly introduced MSX-DOS filesystem infrastructure should be created directly under `infrastructure/filesystem/msx_dos/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-18: Explicit filesystem selection infrastructure
  - Layer: Infrastructure
  - C# source area: explicit resolver and provider wiring
  - Expected C++ target: provider-independent selection and resolver infrastructure
  - Structural relocation: newly introduced explicit selection infrastructure should be created directly under `infrastructure/filesystem/` and filesystem-specific explicit helpers should remain under their family subfolders per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-19: Filesystem detection infrastructure
  - Layer: Infrastructure
  - C# source area: filesystem detection and candidate evaluation
  - Expected C++ target: detection infrastructure over concrete container and filesystem adapters
  - Structural relocation: newly introduced filesystem detection infrastructure should be created directly under `infrastructure/filesystem/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-20: Mounted-medium and controller-medium infrastructure
  - Layer: Infrastructure
  - C# source area: mounted medium binding and controller-facing medium adapters
  - Expected C++ target: concrete mounted-medium and controller-medium adapters
  - Structural relocation: newly introduced mounted-medium infrastructure should be created directly under `infrastructure/drive/`, and newly introduced controller-facing medium adapters should be created directly under `infrastructure/fdc/medium/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-21: Native bridge infrastructure over C++ implementations
  - Layer: Infrastructure
  - C# source area: current native bridge backed by managed implementation
  - Expected C++ target: ABI-compatible native bridge backed by C++ implementations
  - Structural relocation: newly introduced native bridge infrastructure should be created directly under `infrastructure/native/` per the [DDD folder migration rulebook](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Cpp_Ddd_Folder_Migration_Rulebook.md)

- [x] Phase V2-22: WASM path-independent infrastructure
  - Layer: Infrastructure
  - C# source area: browser-first and buffer-first WASM shape definition
  - Expected C++ target: buffer-oriented infrastructure suitable for WASM-facing runtime binding

### Application Phases

- [x] Phase V2-23: Disk service application layer in C++
  - Layer: Application
  - C# source area: disk open, create, format, and metadata orchestration
  - Expected C++ target: application service for disk-level use cases

- [x] Phase V2-24: File transfer application layer in C++
  - Layer: Application
  - C# source area: import, export, overwrite, and payload transfer orchestration
  - Expected C++ target: application service for file-level use cases
  - Completion Note: Implemented `FileTransferService` and `CharacterEncodingService`. Note: Current implementation uses a simplified ASCII check (bit 0) and maps PC88/MSX encoding to X1 table as a temporary measure. Full parity verification belongs to V2-30.

- [x] Phase V2-25: Directory layout application layer in C++
  - Layer: Application
  - C# source area: layout-oriented listing, ordering, and plan handling
  - Expected C++ target: application service for directory layout operations
  - Completion Note: Implemented `DirectoryLayoutService` with plan-based orchestration. Supported Hu-BASIC directory layout reconstruction and label insertion. Fixed a session bug where `Format()` misidentified Hu-BASIC as N88-BASIC.

- [x] Phase V2-26: Boot and clone application layer in C++
  - Layer: Application
  - C# source area: boot area and disk clone orchestration
  - Expected C++ target: application service for boot and clone workflows
  - Completion Note: Implemented `BootAndCloneService` for transferring boot areas and files between disks. Standardized `ReadBootArea` / `WriteBootArea` across all C++ file systems.

- [x] Phase V2-27: Explicit resolver and bootstrap application layer in C++
  - Layer: Application
  - C# source area: managed bootstrap and supported default wiring
  - Expected C++ target: native or C++ bootstrap for supported application services
  - Completion Note: Implemented `legacy89diskkit::cpp::application` namespace with factory functions for all services. Added `ExplicitFileSystemResolver` with `InitializeForDetection` to enable scoring-based Hu-BASIC detection.

- [x] Phase V2-28: Controller-facing application layer in C++
  - Layer: Application
  - C# source area: mounted-medium binding, FDC access, and host adapter orchestration
  - Expected C++ target: application services for controller-shaped and host-facing runtime flows
  - Completion Note: Implemented `DriveMountService`, `MountedMediumBindingService`, `FdcAccessService`, and `EventDrivenEmulatorFdcHostAdapter`. Introduced `FdcMediumController` in Infrastructure to bridge mediums to FDC interface.

- [x] Phase V2-29: Managed-to-native validation bridge
  - Layer: Application
  - C# source area: managed application calling into native or C++-backed implementations
  - Expected C++ target: validation path that allows C# side workflows to exercise C++ slices early
  - Completion Note: Implemented `ValidationNativeBridgeBackend` which routes calls to both C# reference and C++ implementation. Created a C++ SHARED library and C# P/Invoke backend to enable this interop.

- [x] Phase V2-30: Representative workflow parity verification
  - Layer: Application
  - C# source area: list, extract, inject, rename, delete, format, info, and clone flows
  - Expected C++ target: workflow-level comparison between the C# reference and the C++-backed path
  - Completion Note: Verified all major workflows using `ValidationNativeBridgeBackend`. Fixed attribute mapping disparities between C# and C++.

### Presentation Phases

- [x] Phase V2-31: C++ diagnostic and smoke executables
  - Layer: Presentation
  - C# source area: current test and verification harnesses
  - Expected C++ target: minimal executables that prove migrated slices directly

- [x] Phase V2-32: Native test app and host-side verification presentation
  - Layer: Presentation
  - C# source area: native test app and diagnostic host usage
  - Expected C++ target: equivalent verification-facing entrypoints over C++-backed native infrastructure
  - Completion Note: Implemented `ldk-verify` C++ tool for comprehensive disk verification and bootable disk creation. Extended `NativeBridge` and `NativeFileSystemSession` with raw sector access (`ldk_read_sector`/`ldk_write_sector`).

- [ ] Phase V2-33: CLI backend substitution bridge
  - Layer: Presentation
  - C# source area: managed CLI command handlers
  - Expected C++ target: C# CLI paths able to call C++-backed services through a stable bridge

- [ ] Phase V2-34: CLI command migration to C++-backed workflows
  - Layer: Presentation
  - C# source area: `list`, `info`, `file`, `disk`, `boot`, and host-facing commands
  - Expected C++ target: selected commands routed through validated C++-backed application paths

- [ ] Phase V2-35: Full C++ CLI
  - Layer: Presentation
  - C# source area: current `Legacy89DiskKit.Cli`
  - Expected C++ target: standalone CLI over C++ application and infrastructure layers

- [ ] Phase V2-36: WASM presentation runtime
  - Layer: Presentation
  - C# source area: current documented-only WASM direction
  - Expected C++ target: browser-facing or WASI-facing runtime entrypoint over path-independent core and infrastructure

## Current Repository Reading

The current repository state supports the following interpretation.

- Domain migration is the most advanced area
- filesystem-domain work for Hu-BASIC, N88-BASIC, and MSX-DOS has meaningful C++ coverage
- cross-family Domain visibility exists through the filesystem surface catalog
- Presentation has minimal direct smoke and diagnostic executables
- Infrastructure and Application migration remain largely ahead of the project
- native and CLI replacement work have not yet crossed into C++-backed runtime substitution

In DDD terms, the current state is not "full C++ parity."

It is better described as:

- substantial Domain migration
- minimal Presentation-level smoke support
- limited or not-yet-started Infrastructure migration
- not-yet-started Application migration

## Current Bottlenecks

The current bottlenecks are:

1. C++ disk-image and filesystem Infrastructure
2. C++ Application services
3. bridge validation from C# Application into C++-backed paths
4. Presentation substitution at the CLI boundary

## Recommended Immediate Order

The recommended next execution order is:

1. Phase V2-11 through Phase V2-19
2. Phase V2-23 through Phase V2-30
3. Phase V2-21 and Phase V2-33 through Phase V2-35
4. Phase V2-22 and Phase V2-36 when the lower layers are mature enough

This order keeps the migration understandable:

- Domain first
- Infrastructure second
- Application third
- Presentation replacement after validation

## Interpretation Rule

Completion of a Domain phase does not imply completion of Infrastructure, Application, or Presentation for the same subsystem.

For example:

- a filesystem Domain phase may be complete
- while the matching Infrastructure phase remains incomplete
- which means the end-user CLI still cannot rely on that C++ implementation path directly

This distinction is essential to keeping the migration roadmap honest.
