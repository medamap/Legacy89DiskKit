---
name: legacy89-v2-review
description: Reviews Legacy89DiskKit changes against the Roadmap V2 migration rules, DDD layer boundaries, ABI stability, and C# to C++ parity expectations. Use after each V2 phase implementation or when a change touches native bridge, filesystem infrastructure, or roadmap-tracked migration work.
---

# Legacy89 V2 Review

Use this skill when reviewing implementation work in this repository, especially for:

- `Roadmap_V2.md` phases
- C# to C++ migration work
- native bridge changes
- filesystem or container infrastructure
- C++ DDD folder migration

## Read First

Read these in order before reviewing:

1. `AGENTS.md`
2. `Documents/Agent_Handoff_Roadmap_V2.md`
3. `Documents/Roadmap_V2.md`
4. `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`

If the change is phase-scoped, identify the exact V2 phase first.

## Review Goal

The goal is not "is this code acceptable in general?"

The goal is:

- does this change satisfy the intended V2 phase
- does it stay inside the correct DDD layer
- does it avoid regressing already-stable behavior
- does it preserve a clean migration path from C# to C++

## Required Review Questions

Always answer these questions during review.

### 1. Is the change in the correct layer?

Check whether the implementation belongs to:

- Domain
- Infrastructure
- Application
- Presentation

Flag it if:

- Domain absorbs host/path/runtime concerns
- Infrastructure starts doing Application orchestration
- Application starts formatting UI output
- Presentation starts owning filesystem parsing or mutation rules

### 2. Is the change actually inside the declared V2 phase?

Review against the current phase in `Documents/Roadmap_V2.md`.

Flag it if:

- the change silently pulls in responsibilities from a later phase
- the change broadens scope beyond the phase boundary
- the implementation is technically useful but belongs to a different V2 phase

### 3. Does it preserve existing semantics?

This is critical for migration work.

Flag it if:

- a path-based API changes behavior while adding a buffer-based API
- explicit selection semantics change while adding detection
- read-only or write semantics change as a side effect
- file/container format routing changes without a strong reason

When a new entrypoint is added, check that old entrypoints still mean the same thing.

### 4. Does it introduce C++ lifetime or ownership hazards?

Pay special attention to:

- use-after-move
- moved-from object access
- invalid span or pointer lifetimes
- handle table ownership
- buffer length and null handling
- ABI-facing string and struct writes

If a value is moved into storage, do not allow later reads from that same source object.

### 5. Are public headers self-contained?

For any changed public header, verify:

- required STL headers are included directly
- the header does not rely on transitive includes
- exported types are visible without include-order tricks

### 6. Does it preserve ABI stability?

When `include/legacy89diskkit_native.h` or native bridge exports change:

- confirm new exports are added intentionally
- confirm existing exported signatures are not silently broken
- confirm status-code mapping is sane
- confirm handle semantics stay coherent

Flag placeholder exports if they create a misleading "implemented" surface.

### 7. Is the file placement correct under the DDD rulebook?

When new C++ files are added, verify they are under the intended folder:

- `domain/...`
- `infrastructure/...`
- `application/...`
- presentation/test executables where appropriate

If not moved yet, check whether the rulebook should have been updated.

## Project-Specific Red Flags

Treat these as high-suspicion patterns.

- A new buffer-based path changes the old path-based route selection
- Detection logic is broadened inside a phase that is not about detection
- A permissive parser is used as format detection without a format hint
- A native bridge export starts doing more than the current phase requires
- A phase marked as Infrastructure starts resembling `DiskService` or `FileTransferService`
- A review report claims success without matching tests for the new path
- A smoke test only proves "it compiles" but not the specific new behavior

## Preferred Review Output

When findings exist, report:

1. findings first, ordered by severity
2. file and line references
3. why it matters in V2 / DDD terms
4. what should be changed

If there are no findings, still mention:

- what phase was reviewed
- what was validated
- any residual risk or missing deeper verification

## Minimal Verification Expectations

For V2 work, look for relevant verification:

- C++ phases:
  - `cmake -S Cpp -B /tmp/legacy89-cpp-build`
  - `cmake --build /tmp/legacy89-cpp-build`
  - `ctest --test-dir /tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp --output-on-failure`

- Native bridge changes:
  - targeted managed tests around `NativeInterop`
  - targeted C++ smoke for the touched native bridge path

Flag the review if the claimed behavior is not actually covered by tests.

## One-Sentence Review Standard

Review every change as a migration step, not just as isolated code: it must fit the declared V2 phase, the declared DDD layer, and the reference-implementation parity strategy.
