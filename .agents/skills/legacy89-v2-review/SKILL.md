---
name: legacy89-v2-review
description: Reviews Legacy89DiskKit changes against Roadmap V2 as a migration step, with emphasis on DDD layer discipline, C# to C++ parity, ABI safety, semantic regressions, and project-specific failure patterns. Use after each V2 phase implementation or when a change touches native bridge, filesystem infrastructure, application services, or roadmap-tracked migration work.
---

# Legacy89 V2 Review

This skill is for reviewing Legacy89DiskKit changes as **migration work**, not as isolated code.

The standard is:

- the change must fit the declared V2 phase
- the change must stay in the correct DDD layer
- the change must not silently alter already-stable semantics
- the change must keep the path from C# reference behavior to C++ replacement behavior intelligible

## Read First

Read these in order before reviewing:

1. `AGENTS.md`
2. `Documents/Agent_Handoff_Roadmap_V2.md`
3. `Documents/Roadmap_V2.md`
4. `Documents/Cpp_Ddd_Folder_Migration_Rulebook.md`

Then identify the exact V2 phase being reviewed.

Do not start by asking "does this compile?"

Start by asking:

- what responsibility is being migrated
- what layer owns that responsibility
- what semantics must remain stable while this phase lands

## Review Posture

Do not review in a school-solution style.

Do not just check whether the implementation "looks reasonable."

Review with the assumption that migration work usually fails in these ways:

- scope drift into the wrong phase
- leakage across DDD layers
- silent semantic regression of an older entrypoint
- parity claims based only on signatures, not behavior
- C++ ownership or ABI mistakes hidden by passing smoke tests

Your job is to **actively search for the most likely migration mistake**, not to passively summarize the patch.

## Mandatory Review Flow

Follow this order.

### 1. Identify the migration promise

Before reading code deeply, write down:

- V2 phase
- expected layer
- C# source responsibility being replaced
- C++ replacement surface being introduced

If that cannot be stated in one or two lines, the review is already weak.

### 2. Locate the semantic risk

Every V2 change has a most-dangerous regression surface.

Find it first.

Examples:

- buffer-based entrypoint added: risk is that path-based meaning changed
- detection change: risk is that explicit selection or format routing changed
- application service added: risk is that it is only thin forwarding, or that it absorbs infrastructure policy
- native bridge export added: risk is ABI drift, handle drift, or status drift
- folder move: risk is no behavior change, but wrong layer placement or stale include/CMake wiring

If you do not identify the main risk first, the review will become shallow.

### 3. Demand evidence, not claims

A statement such as "parity is preserved" is not evidence.

For every major claim, require one of:

- file and line evidence
- direct code path evidence
- matching test coverage
- explicit residual-risk statement

If a reviewer says "same signature" or "same abstraction," that is not enough.

Parity means behavior, not naming.

### 4. Separate success-path proof from regression proof

A lot of weak reviews only prove the new path works.

This skill requires both:

- the new path works
- the old path still means the same thing

If only the success path is tested, report residual risk or a finding.

## Layer Discipline

Always classify the change by DDD layer.

### Domain

Domain owns:

- rules
- models
- parse/transform logic
- transaction-planning logic
- host-independent controller contracts

Red flag:

- Domain begins to know about paths, files, host handles, CLI formatting, or runtime wiring

### Infrastructure

Infrastructure owns:

- concrete adapters
- container shells
- path/buffer loading
- provider wiring
- runtime-backed bindings
- bridge entrypoints

Red flag:

- Infrastructure starts becoming `DiskService` or `FileTransferService`
- Infrastructure silently embeds workflow policy that belongs in Application

### Application

Application owns:

- use-case orchestration
- workflow composition
- service facades
- explicit control over lower-layer collaborators

Red flag:

- Application is only thin forwarding with no orchestration value
- Application starts formatting output or parsing raw filesystem structures

### Presentation

Presentation owns:

- executables
- CLI commands
- frontend/runtime entrypoints
- view-oriented formatting

Red flag:

- Presentation directly owns filesystem mutation rules or low-level parsing

## Change-Type Review Heuristics

Use the relevant section below based on the patch.

### A. Path API or Buffer API Change

Always ask:

- did the existing path-based API keep the same routing semantics
- did the new buffer path accidentally broaden detection
- is there now a missing format hint problem
- is the change truly path-independent, or just path logic pasted into memory form

Project-specific suspicion:

- a permissive D88 parse is used as implicit detection
- raw images can now be reclassified as D88 because probing moved

### B. Detection or Explicit Selection Change

Always ask:

- did explicit selection remain authoritative
- did detection change outside a detection phase
- did a fallback alter the meaning of an existing stable call path

Project-specific suspicion:

- explicit selection and detection get mixed
- filesystem family best-match logic leaks into unrelated phases

### C. Native Bridge or ABI Change

Always ask:

- did the public C header change intentionally
- are status mappings coherent
- are handle lifecycle semantics still consistent
- are strings and buffers written safely
- is a moved-from object read after handle registration
- are unimplemented exports being exposed too early

Project-specific suspicion:

- ABI surface grows faster than the backing implementation
- placeholder exports make the bridge look more mature than it is

### D. Application Service Addition

Always ask:

- is this real orchestration, or just forwarding
- what C# Application responsibility is actually being mirrored
- are dependencies explicit and minimal
- are null/close/reopen/error states handled

Project-specific suspicion:

- "same method names as C#" is treated as proof of parity
- service is introduced before infrastructure responsibilities are mature

### E. Folder Migration / DDD Placement

Always ask:

- is the file under the correct DDD path now
- if not, was the rulebook updated
- are include and CMake paths still correct

Project-specific suspicion:

- old flat layout grows further even though the phase is already in DDD migration mode

## C++ Hazard Checklist

Always look for these, even if the change is otherwise clean:

- moved-from object access
- use-after-move
- span lifetime errors
- pointer lifetime mistakes
- ownership ambiguity
- stale handle-table state
- missing null or length validation
- public headers missing their own STL includes
- hidden reliance on transitive includes

For this repository, moved-from access and hidden transitive includes are common enough to check every time.

## Legacy89-Specific Red Flags

Treat these as high suspicion by default.

- A new buffer entrypoint changes the old path-based behavior
- Detection is broadened inside a phase that is not about detection
- A parser is used as a de facto format detector without a hint
- A native bridge export does more than the current V2 phase requires
- A phase marked Infrastructure starts looking like `DiskService` or `FileTransferService`
- A parity claim is based on matching signatures instead of matching semantics
- A smoke test proves only compile/success path, not regression resistance
- A file is added under the old flat C++ layout even though a DDD destination is obvious

## What Counts As A Good Review

A good review does all of these:

- names the phase
- names the layer
- identifies the most likely regression surface
- shows concrete evidence from files and lines
- distinguishes confirmed behavior from unverified assumptions
- states residual risk when coverage is incomplete

If there are no findings, you still must say what was actually checked.

## Required Output Shape

Use this output shape when reviewing.

### Findings

List findings first, ordered by severity.

Each finding should include:

- file and line reference
- what is wrong
- why it matters in V2 / DDD / parity terms
- what should change

### Open Questions or Residual Risks

If no bug is confirmed but something is weakly covered, say so explicitly.

Examples:

- success path covered, regression path not covered
- signature parity present, semantic parity not demonstrated
- smoke exists, but no error-path verification exists

### Short Summary

Only after findings.

Summarize:

- phase reviewed
- layer reviewed
- what is solid
- what still needs caution

## Minimal Verification Expectations

For V2 work, expect relevant proof.

### C++ migration phases

Look for:

- `cmake -S Cpp -B /tmp/legacy89-cpp-build`
- `cmake --build /tmp/legacy89-cpp-build`
- `ctest --test-dir /tmp/legacy89-cpp-build/Legacy89DiskKit.Cpp --output-on-failure`

### Native bridge work

Also look for:

- targeted managed tests around `NativeInterop`
- targeted C++ smoke for the touched native bridge route

### Application-layer work

Also check:

- the service is not just forwarding
- state transitions are covered
- invalid/open/close/reopen/error paths were considered

If the implementation claim is broader than the tests, say so.

## Final Rule

Review each patch as a **replacement step in a migration**, not as a standalone feature.

The central question is:

- does this patch make the future C++ system more trustworthy without making the current migration path less trustworthy

If the answer is mixed, report the risk plainly.
