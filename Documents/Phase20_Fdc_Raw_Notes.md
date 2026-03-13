# Phase 20 FDC and Raw Direction Notes

This note captures the current architectural decisions and working assumptions around controller-facing access, future raw magnetic-stream support, and the long-term portability path.

It is not a formal specification. It is a decision record intended to preserve intent while `Phase 20` is still evolving.

## Current Direction

The project now distinguishes between two future access surfaces:

- direct image and filesystem access
- controller-oriented access for emulator integration

Direct image access remains the right surface for tooling, inspection, conversion, and filesystem-aware workflows.

Controller-oriented access is a separate concern. It is intended for emulator integration where software inside the emulated machine performs sector- or controller-level operations rather than calling a host-side filesystem API.

## Controller-Oriented Direction

The current working assumption is that the controller-facing path should follow a floppy-controller style model rather than a filesystem-convenience model.

The first reference point for this direction is an MB8877-style interaction model:

- command and status register behavior
- track, sector, and data register state
- drive and side selection
- IRQ and DRQ style signals
- explicit timing progression

The earliest implementation does not need chip-perfect fidelity. It only needs to preserve the architectural shape of controller-driven interaction.

The preferred near-term behavior is:

- use a minimal command subset first
- keep fidelity work and compatibility investigation on a separate track
- allow early controller-facing adapters to return sector-derived results without pretending to be a complete historical controller

## D88 and Raw Sector Images in the Controller Path

The controller-facing design should allow both sector-container and raw sector-image families to sit behind the same broad contract.

Current concrete adapter families:

- `D88Backed...` for the D88/D77-style sector-container family
- `RawDiskBacked...` for raw sector-image families such as `.2d`

This means:

- D88-backed media may provide controller-facing behavior through a sector-oriented adapter
- raw sector-image media may do the same through a parallel adapter family
- both can coexist before lower-level magnetic-stream support exists

## Raw Magnetic-Stream Direction

The long-term architecture should leave room for lower-level media sources that preserve controller-visible magnetic behavior rather than only decoded sectors.

The intended progression is:

1. sector-based images
2. encoded track-level preservation
3. lower-level sampled or timing-oriented raw signal preservation

The project should not assume that every future disk source can be reduced permanently to a clean side/cylinder/sector table.

Future lower-level support may need to preserve:

- inter-sector gaps
- noise
- malformed or timing-sensitive structures
- copy-protection-relevant physical behavior

## Capture and Preservation Workflow

The expected long-term workflow is:

1. capture controller-visible behavior or lower-level magnetic information from a real drive
2. allow an intermediate working representation during capture and analysis
3. convert into a project-owned long-term preservation container

The project-owned preservation container should remain independent from any temporary working capture representation.

## Provisional Project-Owned Raw Container Naming

Current provisional direction:

- family name candidate: `Legacy 89 Storage`
- extension candidate: `.l89`

This naming is not locked yet. It is a working direction only.

The name is preferred because it can cover:

- sector-derived preservation workflows
- encoded-track preservation
- future lower-level raw signal preservation

## Current Implementation Priorities

The agreed near-term order is:

1. keep `Hu-BASIC` as the first filesystem family for parity work
2. keep the first C++ slice read-only
3. use a minimal controller command subset first
4. defer fidelity-heavy controller research to a dedicated future track
5. keep the raw preservation format name provisional until capture and replay requirements are better understood

## Clarified Meanings

### Parity

In this project, parity means that the future C++ implementation should produce the same observable results as the current C# reference implementation for equivalent operations.

It does not mean error correction.

### Read-Only First

Read-only first means implementation order:

- open
- parse
- inspect
- read sectors and metadata

before write-path reconstruction and mutation-heavy behavior.

It does not mean physical media write-protect semantics.

### Minimal Command Subset

Minimal command subset means the first controller-facing implementation should only support the smallest useful set of controller-style commands and state transitions needed to prove the architecture.

It does not mean full historical controller fidelity.

## Deferred Questions

These questions remain open:

- when the project-owned raw preservation format name and extension should be frozen
- how much MB8877-compatible behavior should be required for the first practical emulator-facing milestone
- whether the first encoded-track preservation step is sufficient before lower-level raw signal storage is introduced
- what concrete proof-of-concept should define the first embedded or bare-metal milestone
