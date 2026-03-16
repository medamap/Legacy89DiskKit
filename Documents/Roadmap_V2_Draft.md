# Roadmap V2 Draft

## Purpose

This document is the first structured rewrite of the C# to C++ migration roadmap using DDD and Onion Architecture terms.

Its purpose is to answer these questions clearly:

- which C# responsibility is being migrated
- which architectural layer it belongs to
- which C++ replacement surface is expected
- whether that migration phase is complete or incomplete

This draft is intentionally phase-heavy. A larger number of smaller, clearer phases is preferred over a smaller number of broad phases that hide architectural meaning.

## Layer Model Used By This Roadmap

Roadmap V2 uses the following layers:

- Presentation
- Application
- Infrastructure
- Domain

Each migration phase should belong to one primary layer, even when it depends on other layers.

## Migration Status Model

This draft uses checklist markers.

- `[ ]` not yet complete
- `[x]` complete

The checklist below now includes an initial evidence-based analysis pass against the current repository state.

Those markers should still be revised later if implementation structure changes, but they are no longer placeholder values.

## High-Level Migration Direction

The intended direction is:

1. keep C# as the reference implementation
2. migrate Domain responsibilities into C++
3. migrate Infrastructure responsibilities into C++
4. add bridge or wrapper paths so C# or test-side code can validate C++ behavior early
5. migrate Application responsibilities into C++
6. replace or supplement Presentation and host-facing entrypoints later
7. expand toward WASM and bare-metal only after the required C++ layers are mature

## Roadmap V2 Phase List

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

- [ ] Phase V2-09: Controller-facing domain contracts
  - Layer: Domain
  - C# source area: FDC, drive, timing, controller-facing medium contracts
  - Expected C++ target: host-independent controller and medium contracts

- [ ] Phase V2-10: Raw preservation domain model
  - Layer: Domain
  - C# source area: raw-direction notes and future preservation model
  - Expected C++ target: encoded-track, metadata, integrity, and conversion domain contracts

### Infrastructure Phases

- [ ] Phase V2-11: Disk image buffer-loading infrastructure
  - Layer: Infrastructure
  - C# source area: path or buffer image loading shells
  - Expected C++ target: buffer-oriented container construction entrypoints

- [ ] Phase V2-12: Raw disk container infrastructure
  - Layer: Infrastructure
  - C# source area: raw container assembly and sector access shells
  - Expected C++ target: concrete raw container adapters over migrated domain rules

- [ ] Phase V2-13: D88 container infrastructure
  - Layer: Infrastructure
  - C# source area: D88 container shell and concrete sector access
  - Expected C++ target: concrete D88 container adapters over migrated parser/domain rules

- [ ] Phase V2-14: Character encoding table infrastructure
  - Layer: Infrastructure
  - C# source area: concrete encoding table and conversion data
  - Expected C++ target: reusable encoding table data and lookup infrastructure

- [ ] Phase V2-15: Hu-BASIC filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: Hu-BASIC filesystem implementation shell
  - Expected C++ target: concrete Hu-BASIC container-backed infrastructure over domain rules

- [ ] Phase V2-16: N88-BASIC filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: N88-BASIC filesystem implementation shell
  - Expected C++ target: concrete N88-BASIC container-backed infrastructure over domain rules

- [ ] Phase V2-17: MSX-DOS filesystem infrastructure
  - Layer: Infrastructure
  - C# source area: MSX-DOS filesystem implementation shell
  - Expected C++ target: concrete MSX-DOS container-backed infrastructure over domain rules

- [ ] Phase V2-18: Explicit filesystem selection infrastructure
  - Layer: Infrastructure
  - C# source area: explicit resolver and provider wiring
  - Expected C++ target: provider-independent selection and resolver infrastructure

- [ ] Phase V2-19: Filesystem detection infrastructure
  - Layer: Infrastructure
  - C# source area: filesystem detection and candidate evaluation
  - Expected C++ target: detection infrastructure over concrete container and filesystem adapters

- [ ] Phase V2-20: Mounted-medium and controller-medium infrastructure
  - Layer: Infrastructure
  - C# source area: mounted medium binding and controller-facing medium adapters
  - Expected C++ target: concrete mounted-medium and controller-medium adapters

- [ ] Phase V2-21: Native bridge infrastructure over C++ implementations
  - Layer: Infrastructure
  - C# source area: current native bridge backed by managed implementation
  - Expected C++ target: ABI-compatible native bridge backed by C++ implementations

- [ ] Phase V2-22: WASM path-independent infrastructure
  - Layer: Infrastructure
  - C# source area: browser-first and buffer-first WASM shape definition
  - Expected C++ target: buffer-oriented infrastructure suitable for WASM-facing runtime binding

### Application Phases

- [ ] Phase V2-23: Disk service application layer in C++
  - Layer: Application
  - C# source area: disk open, create, format, and metadata orchestration
  - Expected C++ target: application service for disk-level use cases

- [ ] Phase V2-24: File transfer application layer in C++
  - Layer: Application
  - C# source area: import, export, overwrite, and payload transfer orchestration
  - Expected C++ target: application service for file-level transfer use cases

- [ ] Phase V2-25: Directory layout application layer in C++
  - Layer: Application
  - C# source area: layout-oriented listing, ordering, and plan handling
  - Expected C++ target: application service for directory layout operations

- [ ] Phase V2-26: Boot and clone application layer in C++
  - Layer: Application
  - C# source area: boot area and disk clone orchestration
  - Expected C++ target: application service for boot and clone workflows

- [ ] Phase V2-27: Explicit resolver and bootstrap application layer in C++
  - Layer: Application
  - C# source area: managed bootstrap and supported default wiring
  - Expected C++ target: native or C++ bootstrap for supported application services

- [ ] Phase V2-28: Controller-facing application layer in C++
  - Layer: Application
  - C# source area: mounted-medium binding, FDC access, and host adapter orchestration
  - Expected C++ target: application services for controller-shaped and host-facing runtime flows

- [ ] Phase V2-29: Managed-to-native validation bridge
  - Layer: Application
  - C# source area: managed application calling into native or C++-backed implementations
  - Expected C++ target: validation path that allows C# side workflows to exercise C++ slices early

- [ ] Phase V2-30: Representative workflow parity verification
  - Layer: Application
  - C# source area: list, extract, inject, rename, delete, format, info, and clone flows
  - Expected C++ target: workflow-level comparison between C# reference and C++-backed path

### Presentation Phases

- [x] Phase V2-31: C++ diagnostic and smoke executables
  - Layer: Presentation
  - C# source area: current test and verification harnesses
  - Expected C++ target: minimal executables that prove migrated slices directly

- [ ] Phase V2-32: Native test app and host-side verification presentation
  - Layer: Presentation
  - C# source area: native test app and diagnostic host usage
  - Expected C++ target: equivalent verification-facing entrypoints over C++-backed native infrastructure

- [ ] Phase V2-33: CLI backend substitution bridge
  - Layer: Presentation
  - C# source area: managed CLI command handlers
  - Expected C++ target: C# CLI paths able to call C++-backed services through a stable bridge

- [ ] Phase V2-34: CLI command migration to C++-backed workflows
  - Layer: Presentation
  - C# source area: `list`, `info`, `file`, `disk`, `boot`, host-facing commands
  - Expected C++ target: selected commands routed through validated C++-backed application paths

- [ ] Phase V2-35: Full C++ CLI
  - Layer: Presentation
  - C# source area: current `Legacy89DiskKit.Cli`
  - Expected C++ target: standalone CLI over C++ application and infrastructure layers

- [ ] Phase V2-36: WASM presentation runtime
  - Layer: Presentation
  - C# source area: current documented-only WASM direction
  - Expected C++ target: browser-facing or WASI-facing runtime entrypoint over path-independent core and infrastructure

## Current Analysis Snapshot

The current repository state supports the following high-level reading:

- Domain migration is the most advanced area
- filesystem-domain work for Hu-BASIC, N88-BASIC, and MSX-DOS has meaningful C++ coverage
- cross-family Domain visibility now exists through the filesystem surface catalog
- Presentation has only minimal direct smoke and diagnostic executables
- Infrastructure and Application migration are still mostly ahead of the project
- native and CLI replacement work have not yet crossed into C++-backed runtime substitution

In DDD terms, the current state is not "C++ parity" in a full-system sense.

It is better described as:

- substantial Domain migration
- minimal Presentation-level smoke support
- limited or not-yet-started Infrastructure migration
- not-yet-started Application migration

## Planned Analysis Pass

The next step after this draft should be a repository-driven analysis pass.

That pass should:

1. inspect current C# and C++ implementation areas
2. map existing code to the phases above
3. change checklist markers from draft values to evidence-based values
4. identify which completed phases are genuinely complete
5. identify which incomplete phases are the active migration bottlenecks

## Important Interpretation Rule

Completion of a Domain phase does not imply completion of Infrastructure, Application, or Presentation for the same subsystem.

For example:

- a filesystem Domain phase may be complete
- while the matching Infrastructure phase remains incomplete
- which means the end-user CLI still cannot rely on that C++ implementation path directly

This distinction is essential to keeping Roadmap V2 honest.

## Immediate Next Documentation Step

The next document update after this draft should be:

- an analysis-backed checklist pass that marks these phases as complete or incomplete based on the current codebase

That next pass should not invent progress. It should only mark phases complete where the current implementation actually satisfies the intended replacement responsibility.
