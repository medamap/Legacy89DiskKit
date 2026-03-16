# C++ DDD Folder Migration Rulebook

## Purpose

This document defines how the C++ implementation should gradually move from its current flat layout toward a DDD-oriented folder structure.

It exists to prevent two problems:

1. migration progress becomes hard to understand because code placement does not reflect the intended architectural layer
2. file moves break include paths, CMake definitions, and tests without a stable record of the intended target location

Use this document together with [Roadmap V2](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/Roadmap_V2.md).

## Core Rule

Each C++ migration phase may contain two kinds of work:

1. implementation work
2. structural relocation work

Structural relocation work means moving touched C++ files into a folder layout that reflects their DDD layer and responsibility domain.

## Why Gradual Relocation Is Required

The current C++ codebase contains a large amount of Domain-oriented work in a flat folder layout.

A single bulk move would create unnecessary risk:

- include path breakage
- CMake path breakage
- test breakage
- poor review readability
- expensive rollback

The preferred strategy is therefore:

- move only files touched by the active phase when the move is low-risk
- keep each relocation small enough to verify immediately
- record every move in the mapping ledger below

## Target Layout

The intended long-term public header layout is:

- `include/legacy89diskkit/cpp/domain/...`
- `include/legacy89diskkit/cpp/infrastructure/...`
- `include/legacy89diskkit/cpp/application/...`
- `include/legacy89diskkit/cpp/presentation/...`

The intended long-term source layout is:

- `src/domain/...`
- `src/infrastructure/...`
- `src/application/...`
- `src/presentation/...`

Tests may remain under `tests/`, but their include paths and naming should reflect the same architectural grouping.

## Layer Assignment Rules

### Domain

A file belongs to `domain/` when it primarily defines:

- value types
- models
- parsing rules
- transaction-planning rules
- filesystem behavior rules
- host-independent controller contracts

### Infrastructure

A file belongs to `infrastructure/` when it primarily defines:

- concrete adapters
- container shells
- path or buffer loading shells
- provider wiring
- runtime construction helpers
- native bridge backing implementations

### Application

A file belongs to `application/` when it primarily defines:

- use-case orchestration
- service facades
- workflow composition
- supported bootstrap and wiring

### Presentation

A file belongs to `presentation/` when it primarily defines:

- CLI handling
- user-facing entrypoints
- host executable entrypoints
- diagnostic executables
- user-facing formatting boundaries

## Relocation Rules

1. Do not move files in bulk unless the current phase explicitly requires it.
2. When a file is moved, update all affected include paths in the same change.
3. When a file move affects CMake definitions, update CMake in the same change.
4. Every moved file should be recorded in the mapping ledger below.
5. If a move is too risky for the current phase, leave the file in place and record it as deferred.

## Mapping Ledger

| Status | Phase | Layer | Current Path | Target Path | Notes |
| --- | --- | --- | --- | --- | --- |
| moved | V2-09 | Domain | `include/legacy89diskkit/cpp/fdc_types.hpp` | `include/legacy89diskkit/cpp/domain/fdc/fdc_types.hpp` | controller-facing FDC model |
| moved | V2-09 | Domain | `include/legacy89diskkit/cpp/fdc_controller_contracts.hpp` | `include/legacy89diskkit/cpp/domain/fdc/fdc_controller_contracts.hpp` | controller contracts |
| moved | V2-09 | Domain | `include/legacy89diskkit/cpp/drive_types.hpp` | `include/legacy89diskkit/cpp/domain/drive/drive_types.hpp` | drive state model |
| created-in-target | V2-09 | Domain | `n/a` | `include/legacy89diskkit/cpp/domain/drive/mounted_medium_contracts.hpp` | mounted medium contract created directly in target layout |
| created-in-target | V2-09 | Domain | `n/a` | `include/legacy89diskkit/cpp/domain/drive/sector_addressable_medium_contracts.hpp` | sector-addressable medium contract created directly in target layout |
| moved | V2-09 | Domain | `include/legacy89diskkit/cpp/controller_runtime_contracts.hpp` | `include/legacy89diskkit/cpp/domain/controller/controller_runtime_contracts.hpp` | medium, drive, and clock contracts |
| created-in-target | V2-10 | Domain | `n/a` | `include/legacy89diskkit/cpp/domain/raw/raw_preservation_types.hpp` | raw preservation identity, metadata, and integrity types |
| created-in-target | V2-10 | Domain | `n/a` | `include/legacy89diskkit/cpp/domain/raw/encoded_track_contracts.hpp` | encoded-track payload and surface contract |
| created-in-target | V2-10 | Domain | `n/a` | `include/legacy89diskkit/cpp/domain/raw/raw_conversion_contracts.hpp` | raw conversion direction and lossiness catalog |
| created-in-target | V2-11 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/disk_image/buffer_image_format.hpp` | buffer image format normalization |
| created-in-target | V2-11 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/disk_image/disk_image_buffer_loader.hpp` | buffer-first loading entrypoint |
| created-in-target | V2-11 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/disk_image/d88_buffer_loader.hpp` | D88 concrete buffer loader |
| created-in-target | V2-11 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/disk_image/raw_buffer_loader.hpp` | raw concrete buffer loader |
| created-in-target | V2-11 | Infrastructure | `n/a` | `src/infrastructure/disk_image/buffer_image_format.cpp` | buffer image format implementation |
| created-in-target | V2-11 | Infrastructure | `n/a` | `src/infrastructure/disk_image/disk_image_buffer_loader.cpp` | buffer-first loading implementation |
| created-in-target | V2-11 | Infrastructure | `n/a` | `src/infrastructure/disk_image/d88_buffer_loader.cpp` | D88 buffer loader implementation |
| created-in-target | V2-11 | Infrastructure | `n/a` | `src/infrastructure/disk_image/raw_buffer_loader.cpp` | raw buffer loader implementation |
| created-in-target | V2-12 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/disk_image/raw_disk_container.hpp` | in-memory raw container adapter |
| created-in-target | V2-12 | Infrastructure | `n/a` | `src/infrastructure/disk_image/raw_disk_container.cpp` | raw container implementation over migrated geometry rules |
| created-in-target | V2-13 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/disk_image/d88_disk_container.hpp` | in-memory D88 container adapter |
| created-in-target | V2-13 | Infrastructure | `n/a` | `src/infrastructure/disk_image/d88_disk_container.cpp` | D88 container implementation over migrated parser and domain rules |
| created-in-target | V2-14 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/character_encoding/byte_text_encoding_table.hpp` | reusable byte-to-text table and lookup surface |
| created-in-target | V2-14 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/character_encoding/x1_encoding_table.hpp` | concrete X1 encoding table surface |
| created-in-target | V2-14 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/character_encoding/character_encoding_table_catalog.hpp` | concrete encoding table catalog surface |
| created-in-target | V2-14 | Infrastructure | `n/a` | `src/infrastructure/character_encoding/byte_text_encoding_table.cpp` | reusable lookup implementation |
| created-in-target | V2-14 | Infrastructure | `n/a` | `src/infrastructure/character_encoding/x1_encoding_table.cpp` | X1 concrete table data |
| created-in-target | V2-14 | Infrastructure | `n/a` | `src/infrastructure/character_encoding/character_encoding_table_catalog.cpp` | concrete table catalog implementation |
| created-in-target | V2-15 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/filesystem/hu_basic/hu_basic_file_system.hpp` | concrete Hu-BASIC filesystem adapter over raw and D88 containers |
| created-in-target | V2-15 | Infrastructure | `n/a` | `src/infrastructure/filesystem/hu_basic/hu_basic_file_system.cpp` | Hu-BASIC filesystem implementation over migrated domain rules |
| created-in-target | V2-16 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/filesystem/n88_basic/n88_basic_file_system.hpp` | concrete N88-BASIC filesystem adapter over raw and D88 containers |
| created-in-target | V2-16 | Infrastructure | `n/a` | `src/infrastructure/filesystem/n88_basic/n88_basic_file_system.cpp` | N88-BASIC filesystem implementation over migrated domain rules |
| created-in-target | V2-17 | Infrastructure | `n/a` | `include/legacy89diskkit/cpp/infrastructure/filesystem/msx_dos/msx_dos_file_system.hpp` | concrete MSX-DOS filesystem adapter over raw and D88 containers |
| created-in-target | V2-17 | Infrastructure | `n/a` | `src/infrastructure/filesystem/msx_dos/msx_dos_file_system.cpp` | MSX-DOS filesystem implementation over migrated domain rules |

Append new rows as additional phases relocate files.

## Workflow For Each Phase

For any Roadmap V2 phase that touches C++ structure, the expected order is:

1. decide whether the touched files should move now or later
2. update this rulebook ledger
3. move the files if the move is low-risk
4. fix include paths and CMake paths
5. run the relevant build and tests

## Immediate Interpretation For Current Work

The current V2-09 work is a suitable candidate for folder relocation because:

- the files are newly introduced
- they are header-only contracts and models
- the include surface is still small
- the move can be validated cheaply through the existing smoke executable

That makes V2-09 an appropriate starting point for enforcing the new folder structure policy.
