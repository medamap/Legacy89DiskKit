# Roadmap V2 Preparation

## Purpose

This document records why the project needs a new migration roadmap and how that roadmap should be constructed.

It exists for one reason: the current roadmap and implementation history describe meaningful work, but they do not expose progress in the same DDD and Onion Architecture terms that the project owner uses for planning and evaluation.

The result is avoidable ambiguity:

- work may be real, but its architectural meaning is hard to see
- C++ progress may appear larger or smaller than it actually is
- "phase complete" can be technically true while still being hard to evaluate from a layer-by-layer migration perspective
- the migration from C# to C++ can look like a continuous stream of isolated ports instead of a structured replacement plan

Roadmap V2 is intended to fix that.

## Why Roadmap V2 Is Needed

The repository currently has:

- a mature C# implementation organized in Domain, Application, Infrastructure, and CLI/Presentation layers
- a growing C++ implementation that currently emphasizes portable core rules and parsing logic
- a native bridge line intended to connect the managed implementation to future native and C++-backed implementations
- longer-term plans for WASM and bare-metal-oriented targets

The current roadmap is useful for broad direction, but it is not ideal for answering these practical questions:

1. Which C# Domain responsibilities have already been ported to C++?
2. Which C# Infrastructure responsibilities are still managed-only?
3. Which C# Application services still have no C++ counterpart?
4. Which future phases are about Domain migration, which are about Infrastructure migration, and which are about Application or Presentation replacement?
5. At what point can C# Application call into C++ safely for real compatibility checks?
6. Which completed phases are actually complete from a DDD migration perspective, and which are only partial slices?

Roadmap V2 is therefore not a cosmetic rewrite. It is a change in planning model.

## Core Principle

Roadmap V2 should describe migration in the same architectural language used to reason about the codebase:

- bounded concerns
- Domain responsibilities
- Infrastructure responsibilities
- Application responsibilities
- Presentation responsibilities

Instead of treating "C++ work" as one large stream, Roadmap V2 should answer:

- which responsibility is being replaced
- from which C# layer
- by which C++ layer
- with what migration outcome
- and with what validation path

## Main Planning Shift

The planning model should move from:

- broad product-oriented phases

to:

- explicit migration phases for individual architectural responsibilities

This means Roadmap V2 should make room for many more phases if needed.

That is acceptable and desirable.

A longer roadmap with smaller, clearer phases is better than a shorter roadmap whose progress cannot be judged reliably.

## Roadmap V2 Must Make These Things Visible

For every migration phase, the roadmap should make it obvious:

1. Which layer the phase belongs to:
   - Presentation
   - Application
   - Infrastructure
   - Domain
2. Which responsibility domain or subsystem it targets:
   - disk image/container
   - filesystem family
   - character encoding
   - controller-facing runtime
   - native bridge
   - WASM-facing path-independent runtime
3. Which C# implementation area it corresponds to
4. Which C++ implementation area is expected to replace or mirror it
5. What counts as completion
6. Whether that phase is already complete, partially complete, or not started

## Immediate Objective

Before rewriting the roadmap itself, the project needs a stable framing document for Roadmap V2.

This document is that framing document.

The immediate objective after this file is created is:

1. define the construction rules for Roadmap V2
2. define the migration analysis method
3. create the Roadmap V2 phase list
4. mark which phases are complete and which are incomplete

## Construction Rules For Roadmap V2

Roadmap V2 should be built using the following rules.

### Rule 1: Start From The C# Reference Structure

Roadmap V2 should begin from the current C# implementation as the source structure to be migrated.

That means the roadmap should inspect C# responsibilities such as:

- Domain models and rules
- Infrastructure adapters and parsers
- Application services and orchestration
- CLI and presentation behavior

The roadmap should then ask how each responsibility should appear in C++.

### Rule 2: Do Not Treat C++ Progress As A Single Track

C++ progress must not be described as one vague "core migration."

Instead, migration must be separated into responsibility tracks such as:

- disk image domain migration
- filesystem domain migration
- character encoding domain migration
- disk image infrastructure migration
- filesystem infrastructure migration
- application-service migration
- presentation/frontend migration

### Rule 3: Allow Non-Sequential Completion

Roadmap V2 should assume that phases may complete out of order.

This is important because the migration will likely produce a pattern like:

- some Domain phases complete early
- some Infrastructure phases lag behind
- some Application phases remain blocked until C++ Domain and Infrastructure are mature
- some Presentation replacement phases remain late

Therefore Roadmap V2 should use checklist markers such as:

- `[x]` complete
- `[ ]` incomplete

without assuming that every phase must complete in strict numeric order.

### Rule 4: Separate Migration Completion From Production Replacement

A phase may be complete even if the end-user product has not yet switched to C++.

For example:

- a C++ Domain phase can be complete
- a C++ Infrastructure phase can be complete
- but the CLI may still be using C# until validation and bridge work are ready

Roadmap V2 must preserve that distinction.

### Rule 5: Add Early Validation Gates

Whenever a meaningful C++ Domain and Infrastructure slice exists, Roadmap V2 should plan an early validation path.

That usually means:

- creating a bridge or wrapper so C# Application or another verification layer can call the C++ implementation
- checking compatibility against the known C# behavior
- detecting drift early instead of after a full rewrite

This validation step is not optional. It is part of the migration strategy.

## Why Early Validation Matters

Without early validation, the project risks:

- building a large C++ implementation that behaves differently from the C# reference
- discovering incompatibilities too late
- losing confidence in parity claims
- making rollback or correction more expensive

With early validation, the project can:

- compare C++ behavior to the current C# reference implementation
- keep the existing CLI as a verification harness during migration
- adopt C++ incrementally rather than all at once
- reduce the cost of mistakes

## Expected Shape Of The Migration

Roadmap V2 should assume a staged replacement model rather than a big-bang rewrite.

The expected shape is:

1. C# remains the reference implementation
2. C++ Domain responsibilities are migrated
3. C++ Infrastructure responsibilities are migrated
4. a bridge layer allows managed or test-side callers to exercise C++ behavior
5. compatibility is checked early
6. Application responsibilities begin moving
7. Presentation/frontend replacement happens later
8. only after that does a fully native or bare-metal-oriented path become realistic

## Relation To WASM And Bare-Metal Work

Roadmap V2 should not pretend that WASM or bare-metal work comes directly after "some C++ files exist."

Instead, it should treat them as downstream of architectural migration.

That means:

- WASM depends on path-independent and portable layers being sufficiently mature
- bare-metal depends on C++ Domain and Infrastructure being sufficiently mature
- neither should be described as "the next step" unless the necessary migration layers are already in place

## Deliverables That Roadmap V2 Should Eventually Produce

Roadmap V2 itself should eventually provide:

1. a new phase list based on DDD and Onion Architecture responsibilities
2. explicit mapping from C# areas to C++ replacement areas
3. completion markers for each phase
4. a clear distinction between:
   - already complete
   - partially complete
   - not started
5. a migration-oriented view of:
   - C++ progress
   - Native bridge progress
   - future WASM readiness
   - future Presentation replacement readiness

## Immediate ToDo For The Roadmap V2 Work

The next work items should be:

- create the Roadmap V2 phase framework
- enumerate C# responsibilities to be migrated
- classify each responsibility by layer:
  - Presentation
  - Application
  - Infrastructure
  - Domain
- map those responsibilities to expected C++ headers, source modules, services, or wrappers
- create a checklist-style phase list
- inspect the current repository state and mark which phases are already complete
- identify the first unfinished phase that should become the active implementation target

## What This Document Is Not

This document does not define the final Roadmap V2 phase list yet.

It only defines:

- why Roadmap V2 is necessary
- how Roadmap V2 should be constructed
- what analytical method should be used
- what the next documentation step must be

That separation is intentional. It reduces the chance that the roadmap rewrite itself becomes too large and loses clarity.

## Summary

Roadmap V2 is needed because the current roadmap does not expose C# to C++ migration progress in the same DDD and Onion Architecture terms used to reason about the system.

The new roadmap should therefore:

- begin from C# layer responsibilities
- map them explicitly to C++ migration targets
- allow out-of-order completion
- include completion checkboxes
- require early compatibility validation
- make it obvious what is actually done and what is still missing

This document should be treated as the stable starting point for the Roadmap V2 rewrite.
