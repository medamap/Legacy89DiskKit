# Phase 24 Real Emulator Integration Plan

## Purpose

This document defines the first real-host integration phase after the external host exposure work.

The goal is to move from in-repository protocol and adapter proofs to out-of-repository emulator bridges without expanding the shared controller/core contract unless a real host proves that expansion is necessary.

## Scope

This phase covers:

- the first real event-driven emulator host bridge
- the second real global-state-style emulator host bridge
- the success criteria for the first real-host proof
- the request and response surface that Phase 24 should treat as fixed until evidence demands change

This phase does not cover:

- high-fidelity MB8877 behavior beyond the current research findings
- raw magnetic stream replay
- write-capable controller flows
- permanent network daemon design
- browser-side runtime integration

## Fixed Phase 23 Baseline

Phase 24 starts from the already shipped host-facing contract.

The baseline request set is:

- `QueryCapabilities`
- `OpenDiskPath`
- `OpenDiskImage`
- `CloseDisk`
- `SelectDrive`
- `SelectSide`
- `Reset`
- `WriteRegister`
- `ReadRegister`
- `Advance`
- `QueryState`

The baseline host-facing transport forms are:

- plain request and response
- notification-aware exchange
- line-delimited text session
- stdio-oriented runner
- CLI `host stdio`

Phase 24 should treat this as the stable starting point.

## Real Host Order

The first two real-host targets remain:

1. an event-driven emulator host bridge
2. a more global-state-style emulator host bridge

The first target is preferred because it is closer to the current explicit timing and delayed-callback model.

## First Real Event-Driven Host Proof

The first real-host proof should demonstrate:

- the host can perform `QueryCapabilities`
- the host can choose path-based or buffer-based disk open without changing protocol shape
- the host can drive `restore`, `seek`, `read sector`, and `force interrupt` through the current narrow controller path
- the host can consume busy, IRQ, DRQ, and `record-not-found` style outcomes without host-specific protocol forks
- the host can schedule delayed advancement outside this repository

The first proof should remain:

- read-only
- D88-backed first
- raw sector-image-backed second
- separate-process or IPC-friendly where direct linking is undesirable

## Second Real Host Proof

The second real-host proof should demonstrate:

- the same request set still works for a host with more global state and C-style entrypoints
- the host can wrap its own event-object scheduling around `Advance` and the current notification signals
- the host can share the same mounted-medium and controller-facing baseline without a protocol fork

## Success Criteria

Phase 24 repository work is complete when:

1. the fixed request set is frozen as the first-contact baseline for external host work
2. shipped tooling can generate request scripts, inspect transcripts, pack bundles, and verify bundle or transcript results
3. bridge-side checklists, task lists, and report templates remain available for external host work
4. out-of-repository emulator bridge execution is explicitly marked as pending external validation
5. any required protocol expansion remains evidence-driven rather than speculative

## Failure Criteria

Phase 24 should not hide integration friction.

If a real host requires more than the fixed baseline request set, the phase should record:

- which request or notification is missing
- whether the missing piece is host-specific or generally reusable
- whether the expansion belongs in Phase 24 or should be deferred

This phase should prefer documenting proven gaps over inventing speculative protocol growth.

## External Validation Status

The following work remains intentionally outside this repository phase closure:

- the first real event-driven host bridge execution
- the second real global-state-style host bridge execution
- any transcript or bundle produced by those real hosts

Those results should be carried back later through the Phase 24 tooling rather than blocking the current mainline roadmap.
