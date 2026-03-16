# Agent Handoff for Roadmap V2 Work

## Purpose

This document is the single-entry handoff note for the next coding agent.

If a new agent is asked to continue the current C# to C++ migration work, the user should be able to say:

- "Read this file and continue the work."

and that should be enough to resume correctly.

## Read Order

Use the following order when resuming:

1. current code and current branch state
2. `Documents/Roadmap_V2.md`
3. `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`
4. `Documents/Roadmap_V2_Preparation.md`
5. `Documents/ROADMAP.md`
6. `Documents/handoff/task.md`

If documents disagree, prefer the current code and `Roadmap_V2.md` for the current migration track.

## What The User Wants

The user wants the migration to be tracked in DDD / Onion terms.

That means work should be described and executed in terms of:

- Domain
- Infrastructure
- Application
- Presentation

The user does not want broad, vague phases that keep expanding with "just a little more" work.

The user wants:

- small, explicit migration phases
- a clear source C# responsibility
- a clear C++ replacement target
- a clear completion check
- phases that can be marked complete and closed without drift

## Current Planning Model

The active migration plan is `Documents/Roadmap_V2.md`.

That file is now the practical roadmap for this work.

It exists because the earlier broad roadmap style made progress hard to evaluate.

Roadmap V2 fixes that by tracking:

- which C# responsibility is being replaced
- which architectural layer it belongs to
- which C++ target should exist
- whether the phase is complete

## Structural Rule

When touching C++ files, also check whether they should be placed under the DDD-oriented folder layout described in:

- `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`

The rulebook is not optional.

If a phase introduces new C++ files, prefer placing them directly in the correct DDD-oriented folder when low-risk.

If a file is not moved yet, record the migration note in the rulebook so later agents can see the intended destination.

## Current Migration Status

As of this handoff, the following are complete in `Roadmap_V2.md`:

- `V2-01` through `V2-10` for Domain
- `V2-11` through `V2-21` for Infrastructure
- `V2-31` for Presentation

This means:

- C++ Domain has a large amount of migrated logic
- C++ Infrastructure has now reached the native bridge slice
- C++ Application has not started yet in V2 terms
- C++ Presentation is still limited to smoke and diagnostic executables

## Current Branch Situation

At the time of this handoff:

- current integration branch is `develop`
- `develop` already contains `V2-21`
- current `develop` HEAD was `1749804` when this file was written

## Most Recent Completed Phase

The latest completed phase is:

- `V2-21: Native bridge infrastructure over C++ implementations`

That phase is considered complete because:

- C# `NativeInterop` no longer assumes only a managed backend shape
- backend provider and session abstractions exist
- C++ has native-session-backed infrastructure and ABI-shaped native bridge entry points
- C++ smoke coverage for the native bridge slice is green

Important:

- `V2-21` does **not** mean that the managed CLI is already calling the C++ backend
- that later validation and application-level bridge work belongs to later V2 phases, especially `V2-29`

## Next Recommended Phase

If work continues strictly by layer order, the next phase is:

- `V2-22: WASM path-independent infrastructure`

After Infrastructure finishes, move into Application phases:

- `V2-23` onward

## How To Execute Work

For each new phase:

1. create a new branch from `develop`
2. name it with the `codex/` prefix and the V2 phase identifier
3. implement only the responsibility of that phase
4. keep the completion boundary narrow
5. add or extend tests or smoke coverage
6. update `Documents/Roadmap_V2.md` when the phase is truly complete
7. update `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md` when file placement or relocation notes change
8. merge to `develop` with `--no-ff`
9. push `develop`

Do not let a phase sprawl into the next layer unless the user explicitly redirects.

## Branching Rules

Always:

- branch from `develop`
- use the `codex/` prefix
- use `--no-ff` merge into `develop`
- push `develop` after merge

Recommended naming pattern:

- `codex/v2-22-wasm-path-independent-infrastructure`
- `codex/v2-23-disk-service-application-layer`

If a phase finishes earlier than expected, stop early and report that it is complete.

## Testing Rules for This Track

Run relevant tests before and after changes.

For C++ migration phases, the standard verification path is:

```bash
cmake -S Cpp -B /tmp/legacy89-cpp-build
cmake --build /tmp/legacy89-cpp-build
ctest --test-dir /tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp --output-on-failure
```

For C# native bridge related changes, also run targeted managed tests when relevant:

```bash
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false --filter "NativeDiskExportsTest|NativeFileExportsTest|NativeHandleExportsTest|NativeHandleManagerMetadataTest|NativeDiskSessionHandleManagerTest|NativeBackendIdentityTest|NativeBridgeBackendRoutingTest|NativeOpenDiskExportsTest|NativeCreateDiskExportsTest|ManagedPublicSurfaceTest"
```

If a later phase touches broader managed behavior, run the full managed test suite:

```bash
dotnet test CSharp/Legacy89DiskKit.Tests/Legacy89DiskKit.Tests.csproj /p:UseAppHost=false
```

## Practical Warnings

- Keep explanations in DDD / layer terms, because that is how the user is steering the work now
- Do not describe broad product progress in a way that hides which layer actually moved
- Do not reopen already-completed V2 phases unless there is a real defect
- When adding C++ files, place them under the DDD-oriented folder layout when feasible instead of extending the old flat layout
- When a phase is complete, update the roadmap immediately so progress remains visible

## Minimal Resume Instruction

If a new agent needs the shortest possible resume instruction, use this:

- Read `Documents/Agent_Handoff_Roadmap_V2.md`, then `Documents/Roadmap_V2.md`, then `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`, branch from `develop`, and continue with the next incomplete V2 phase.
