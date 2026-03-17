---
name: legacy89-v2-implementation
description: Guides Legacy89DiskKit implementation work under Roadmap V2. Use when implementing a V2 phase and you need project-specific advice on phase scoping, DDD layer discipline, migration strategy, completion boundaries, and safe progress without drifting into later phases.
---

# Legacy89 V2 Implementation

Use this skill when implementing work in this repository under `Documents/Roadmap_V2.md`.

This is not a coding-style guide.

This skill exists to shape implementation judgment:

- what to implement now
- what not to implement yet
- how to keep a phase small and finishable
- how to avoid breaking migration trust

## Read First

Before implementing, read these in order:

1. `AGENTS.md`
2. `Documents/Agent_Handoff_Roadmap_V2.md`
3. `Documents/Roadmap_V2.md`
4. `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`

Then identify the exact V2 phase you are implementing.

## Core Mindset

You are not "building the whole C++ system."

You are replacing **one C# responsibility** with **one C++ replacement surface** inside **one DDD layer**.

The implementation should feel narrow, intentional, and finishable.

If a patch starts to feel broad, it is probably crossing phase boundaries.

## First Questions To Ask

Before writing code, answer these:

1. What exact V2 phase is this?
2. What exact C# responsibility is being replaced?
3. What DDD layer owns that responsibility?
4. What is the smallest C++ surface that honestly satisfies the phase?
5. What must not change in existing behavior while this lands?

If you cannot answer these clearly, do not start coding yet.

## Default Implementation Strategy

### 1. Preserve old meaning, add new surface

When adding a new entrypoint:

- keep old entrypoints meaning the same thing
- add the new path beside them
- avoid hidden rewrites of stable behavior

Examples:

- adding buffer-first must not silently change path-based routing
- adding detection must not weaken explicit selection
- adding a service must not change lower-layer semantics by accident

### 2. Prefer minimal replacement surfaces

Do not port entire subsystems if the phase only needs a narrow slice.

Build the smallest surface that is enough to say:

- this responsibility now exists in C++

Not:

- this might help three later phases too

### 3. Keep phases closable

A V2 phase should be implemented so that it can stop cleanly.

That means:

- a narrow set of files
- a narrow test surface
- a narrow roadmap update
- a clear yes/no completion call

If the work needs "just one more adjacent feature" to feel satisfying, that is a warning sign.

### 4. Let later phases stay later

Do not pre-implement later phases "for convenience."

This project works better when:

- Domain stays Domain
- Infrastructure stays Infrastructure
- Application stays Application
- Presentation waits until lower layers are ready

If a missing dependency belongs to a later phase, stop and say so rather than smuggling it in.

### 5. Treat prerequisite gaps explicitly

Sometimes a phase cannot be completed because a lower-layer prerequisite is missing.

When that happens, do not pretend the current phase is complete.

Instead, decide which of these is true:

- the missing piece is small, local, and still honestly part of the current phase
- the missing piece belongs to a different V2 phase and should stay separate

If you must add lower-layer support to finish the current phase, keep it minimal and make the reason explicit:

- what prerequisite was missing
- why it blocked the current phase
- why the added lower-layer work is the smallest honest unblocker

Do not use "needed for completion" as an excuse to quietly absorb a whole neighboring phase.

## Layer Discipline During Implementation

### Domain

Implement here only:

- rules
- models
- parser logic
- transaction planning
- host-independent contracts

Avoid:

- file paths
- runtime handles
- native exports
- service orchestration

### Infrastructure

Implement here:

- concrete adapters
- buffer/path loading shells
- container-backed implementations
- bridge backing layers
- provider wiring

Avoid:

- broad workflow policy
- UI behavior
- pretending to be Application services without orchestration value

### Application

Implement here:

- use-case orchestration
- service facades
- state transitions across lower-layer collaborators

Avoid:

- raw parser details
- CLI formatting
- meaningless thin wrappers that add no orchestration

### Presentation

Implement here:

- CLI/runtime entrypoints
- executable surfaces
- formatting for users

Avoid:

- filesystem rules
- mutation logic
- low-level parsing

## Project-Specific Warnings

These are common failure modes in this repository.

### 1. D88 vs raw drift

Do not casually broaden format probing.

If a path-based route used extension or explicit choice before, keep that behavior stable unless the phase is explicitly about changing detection semantics.

### 2. Signature parity is not behavior parity

Matching a C# method name or signature does not prove migration success.

Parity means:

- same meaning
- same failure behavior where relevant
- same state transitions where relevant
- same persistence behavior where relevant

Examples:

- `CreateDisk(path, ...)` is not parity if it only creates an in-memory object but the C# reference writes a real file
- `OpenFromBuffer(...)` is not parity if it changes the meaning of existing path-based open
- `Format()` is not parity if it succeeds only inside the current session but leaves no reopenable result when the C# path persists data

When the C# reference performs a real side effect, the C++ replacement must either perform the same side effect or explicitly remain incomplete.

### 3. Thin wrapper trap

When implementing Application phases, do not stop at a class that merely forwards calls.

Ask:

- what orchestration exists here
- what state or workflow rule lives here
- why is this not just Infrastructure in disguise

If the answer is "it mostly forwards, but it owns open/close/reset/reopen/error orchestration," that can still be valid Application work.

If the answer is only "it exposes a nicer method surface," it is probably not enough.

### 4. Native bridge expansion drift

If touching native bridge code:

- do not add exports just because they are nearby
- do not widen ABI surface without a phase reason
- do not claim readiness beyond what the backing implementation actually supports

### 5. Folder drift

If you add a new C++ file, check the rulebook.

Do not keep extending the old flat structure out of habit when the DDD destination is obvious.

### 6. False-positive test trap

Green tests are not useful if they fail to observe the actual promise of the phase.

Before trusting a new smoke test, ask:

- does this test observe the external effect that the phase claims to implement
- could this test still pass if the implementation only updated in-memory state
- does this test verify the same thing a user or upper layer depends on

Examples:

- for create flows, verify the file exists on disk if the contract is path-based
- for write/format flows, verify the result can be reopened and observed again
- for detection/selection flows, verify the chosen family or format, not just that "something opened"

If a test would still pass under a fake or weaker implementation, it is not enough.

### 7. Workspace cleanliness is part of correctness

Tests and smokes must not leave repo-root artifacts behind.

Prefer:

- temporary directories
- unique temp file names
- explicit cleanup in success and failure paths

Do not rely on "we remove it at the end" if assertion failures or crashes can skip cleanup.

An implementation is not cleanly complete if its test routinely leaves untracked files in the repository.

## Safe Progress Pattern

For most V2 phases, the safest order is:

1. define the smallest target surface
2. add headers in the correct DDD folder
3. add implementation
4. add smoke or focused tests
5. run targeted verification
6. decide if the phase is actually complete
7. only then update roadmap/rulebook

If completion is not honest yet, do not mark it complete.

## Completion Discipline

A phase is complete only when the intended responsibility is genuinely present.

Do not mark a phase complete because:

- "the main part is there"
- "the signature matches"
- "tests are green"

Mark it complete only if the roadmap sentence is honestly satisfied.

Ask these before checking `[x]`:

- if the roadmap says `open, create, format, and metadata orchestration`, do all four really exist
- if the C# source area performs persistence, does the C++ path also persist
- if the phase claims a user-visible effect, is that effect directly verified
- if a lower-layer hole still exists, is this really complete or only "almost complete"

If any answer is "not yet," keep the phase open.

If an important listed responsibility is missing, leave the phase open.

If you discover during implementation that a completion claim was too optimistic, reopen the phase and say so plainly.

Correcting a false completion is better than defending it.

## Testing Expectations

At minimum, verify:

- the new path works
- the old path still means the same thing
- obvious error paths behave sensibly
- state cleanup/reset behavior is correct if the phase owns state

For C++ phases, prefer:

```bash
cmake -S Cpp -B /tmp/legacy89-cpp-build
cmake --build /tmp/legacy89-cpp-build
ctest --test-dir /tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp --output-on-failure
```

If touching native bridge or managed/native interop boundaries, also run relevant targeted managed tests.

## When To Stop

Stop implementation and report instead of continuing when:

- the work clearly spills into another V2 phase
- you need a later-layer abstraction to proceed honestly
- the existing semantics would need to change to make the new path pass
- the roadmap completion sentence is not yet satisfiable

Also stop and report when:

- the phase needs a lower-layer prerequisite that is larger than a narrow unblocker
- you are about to change stable behavior just to keep the phase moving
- you can only justify completion by stretching the roadmap wording

Stopping at the right boundary is a success in this project.

## One-Sentence Standard

Implement each patch as a small, truthful migration step that makes Roadmap V2 more complete without making the migration boundary less trustworthy.
