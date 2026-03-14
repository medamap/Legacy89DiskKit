# Phase 21 Emulator Host Integration Plan

## Purpose

This document defines the first host-integration track after the `Phase 20` portability work.

The goal is not to build a universal emulator adapter. The goal is to prove that the shared narrow controller/core contract can be connected to more than one emulator-side integration style through thin host-specific adapters.

The first host-integration track must also avoid implying that this repository is designed for one specific emulator codebase only. The integration boundary should stay generic enough that different emulator hosts can connect through the same narrow contract without turning this repository into a host-specific derivative work.

## Scope

This plan covers:

- the first emulator-hosted integration target
- the second emulator-hosted integration target
- the responsibilities that belong to host adapters
- the minimum read-only proof target

This plan does not cover:

- high-fidelity controller behavior research
- write-capable controller behavior
- raw magnetic stream replay
- board-specific or bare-metal implementation work

Those remain downstream or on separate research tracks.

## Host Order

The first two host targets are:

1. an event-driven host adapter matching a controller shape with delayed callbacks and explicit register access
2. a second host adapter matching the X millennium / xmil-web integration style

The first target is preferred because that controller-facing shape is clearer and closer to the current `Drive / Fdc / Timing` split.

## Shared Core Contract

The host adapters should connect to the existing narrow controller/core contract rather than bypass it.

The shared contract currently includes:

- mounted medium binding
- sector-addressable and controller-facing media abstractions
- controller register access
- visible controller state
- selected drive and selected side visibility
- IRQ and DRQ visibility
- explicit timing advancement

The host adapters must treat this contract as the only stable integration point.

## No Universal Adapter

Phase 21 should not attempt to build one adapter that directly supports every emulator host.

The intended structure is:

- one shared narrow controller/core contract
- one thin adapter per host

That means:

- the host adapter owns host-specific event or timer registration
- the host adapter owns host-specific mount and unmount glue
- the host adapter owns host-specific IRQ and DRQ bridging
- the host adapter owns host-specific drive and side selection glue
- the controller/core contract remains host-agnostic
- the repository must not rely on a host-specific link boundary to justify the existence of the adapter path

## Event-Driven Emulator Host Adapter

### Observed Host Shape

The first target host family exposes a controller object with:

- disk open and close operations
- disk inserted queries
- register read and write operations
- signal input and output hooks
- delayed event callbacks
- IRQ and DRQ output signaling

### Mapping Direction

The thin adapter for this host should translate:

- disk open and close requests into mounted-medium binding and drive mount operations
- register reads and writes into controller register access
- delayed event scheduling into explicit timing advancement
- IRQ and DRQ output hooks into the shared visible-state and signal model
- drive select, side select, and ready state into the shared drive model

### Interface Mapping

| Host-side shape | Legacy89DiskKit side |
| --- | --- |
| disk open and close | mounted-medium binding + drive mount service |
| disk inserted query | drive-ready and mount-state query |
| register read and write | `IFdcController` register access |
| signal input and output | drive-selection, side-selection, IRQ, DRQ bridge |
| delayed event callback | `IControllerClock` + explicit timing advancement |
| public drive type / media queries | `DiskType` and mounted-medium metadata |

### First Adapter Work Package

The first event-driven emulator adapter should be implemented in this order:

1. drive mount and unmount glue
2. register read and write bridge
3. delayed event to timing-advancement bridge
4. IRQ and DRQ bridge
5. ready-state and selected-drive bridge

The first implementation should avoid write-capable controller behavior and should not attempt full chip fidelity.

### Current Managed Progress

The managed reference implementation already includes a first event-driven emulator host adapter scaffold with:

- mounted-medium binding for D88-backed and raw sector-image-backed containers
- drive insert and eject operations
- register-shaped read and write entrypoints
- selected drive and side control
- explicit read-only timing advancement through the shared controller/core contract
- callback-friendly advance-delay hints for host-side event registration

The remaining event-driven emulator work is host-specific glue:

- host event-manager integration using the current advance-delay hints
- richer host event bridge work beyond the current IRQ and DRQ callback path
- process-separated or IPC-friendly integration so emulator-specific bridge code can remain outside this repository when required by license constraints
- a transport or message boundary that remains reusable for more than one emulator host

### First Proof Target

The first proof target for this host should be:

- mount a D88-backed medium
- expose register-shaped access
- support restore, seek, and read-sector in the narrow command subset
- expose busy, IRQ, DRQ, and record-not-found style outcomes
- keep the integration read-only
- keep the host-specific bridge out of the portable core repository when a copyleft or otherwise restrictive emulator license requires process or IPC separation
- avoid naming or structuring the bridge as if it existed for one emulator family only

## xmil-web-Style Host Adapter

### Observed Host Shape

The second host exposes a more C-style integration shape with:

- global controller state
- global register and busy state
- host event objects for busy and read/write progression
- D88 and raw sector-image backends
- port-oriented read and write entrypoints

### Mapping Direction

The thin adapter for this host should translate:

- global register entrypoints into controller register access
- host event objects into timing progression callbacks or steps
- backend mount operations into mounted-medium binding
- global drive and media state into the shared drive model
- busy and transfer progression into the shared controller-visible state

### Interface Mapping

| Host-side shape | Legacy89DiskKit side |
| --- | --- |
| global `x1_fdc_w` / `x1_fdc_r` entrypoints | `IFdcController` register write and read |
| busy and read/write event objects | timing advancement and controller busy state |
| backend mount and eject functions | mounted-medium binding + drive mount service |
| global media and drive fields | mounted drive state and metadata |
| direct sector helper for backend access | sector-addressable medium bridge |

### Second Adapter Work Package

The xmil-web-style adapter should be implemented after the first event-driven host proof.

The second adapter should focus on:

1. global-state bridge isolation
2. event-object to timing-advancement bridge
3. register entrypoint wrapping
4. D88-backed proof
5. raw sector-image-backed proof

### Second Proof Target

The second proof target should remain read-only and should confirm that the shared narrow contract can also fit a host with:

- global state
- C-style entrypoints
- event-object scheduling

This target should come after the first event-driven host adapter proof, not before it.

## Adapter Responsibilities

Every Phase 21 host adapter should explicitly own:

- host-to-drive mount and unmount glue
- drive selection and side selection translation
- ready-state translation
- write-protect visibility translation
- IRQ and DRQ bridge logic
- host timer or event bridge logic
- host-facing register read and write entrypoints

The shared core should not own:

- host event registration APIs
- host thread or loop management
- host-specific global state conventions
- host-specific callback signatures

## Minimum Phase 21 Proof Target

The minimum proof target for emulator-hosted integration is:

- read-only mounted-medium integration
- register-shaped controller access
- explicit host-driven timing progression
- visible busy, IRQ, and DRQ state
- visible drive and side selection state
- D88-backed media support first
- raw sector-image-backed media support second

The minimum proof target explicitly excludes:

- write-sector support
- write-track support
- high-fidelity controller timing
- lower-level raw magnetic stream replay

## Deliverables

Phase 21 should produce:

- a concrete host-integration mapping for the first event-driven host
- a concrete host-integration mapping for the second host style
- a thin-adapter responsibility checklist
- a first proof target definition that can be implemented without changing the narrow controller/core contract
- an implementation order for the first host adapter
- an implementation order for the second host adapter

## Exit Criteria

The planning portion of Phase 21 is complete when:

- the first host adapter target is fixed
- the second host adapter target is fixed
- the shared contract and thin-adapter split are explicit
- the minimum read-only emulator proof target is fixed
- the remaining work can be tracked as adapter implementation tasks rather than architecture questions
