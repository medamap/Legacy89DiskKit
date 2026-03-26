# Phase 24 First Event-Driven Host Bridge Tasks

## Purpose

This document turns the first real event-driven host proof into an executable bridge-side task list.

The focus is not on adding more protocol features inside this repository. The focus is on building the smallest host-side bridge that can drive the existing contract and report whether the contract is sufficient.

## Bridge-Side Work Order

### 1. Launch and Lifetime

- launch `Legacy89DiskKit.Cli host stdio`
- keep stdin and stdout connected for line-delimited JSON exchange
- decide whether stderr should be logged or ignored by the host bridge
- confirm process shutdown behavior on normal EOF and on host-side cancellation

### 2. Handshake

- send `QueryCapabilities`
- verify `ProtocolVersion`
- record whether the bridge will use:
  - `OpenDiskPath`
  - or `OpenDiskImage`
- record whether the bridge will use:
  - plain request and response
  - or notification-aware exchange

### 3. Disk Open Strategy

Choose one initial path:

- path-based open
  - simplest when the emulator-side host can access the same filesystem path
- buffer-based open
  - preferred when the bridge should not assume shared filesystem access

The first proof only needs one of these. The second can be tested after the first path works.

### 4. Register Bridge

- map the host controller write path to:
  - `SelectDrive`
  - `SelectSide`
  - `WriteRegister`
- map the host controller read path to:
  - `ReadRegister`
  - `QueryState` when a non-destructive view is needed

The bridge should not invent extra register commands before testing the existing request set once.

### 5. Timing Bridge

- map the host delayed-event or scheduled-callback mechanism to `Advance`
- if the host already has a usec or msec scheduling primitive, reuse it
- when notification-aware exchange is available, use advance-request notifications to schedule the next callback
- when plain request and response are used, fall back to `PendingAdvanceMicroseconds`

### 6. Signal Bridge

- map IRQ visibility from:
  - notification-aware exchange first
  - or response-visible state when polling is sufficient
- map DRQ visibility the same way
- record whether the host expects edge-triggered handling, level-triggered handling, or polling-only handling

### 7. D88-Backed First Proof

- open one known D88 image
- select drive and side
- issue track and sector writes
- issue read-sector command
- drive time forward externally
- read data register bytes
- close disk

### 8. Raw Sector-Image Follow-Up

After D88 succeeds:

- repeat the same flow with a raw sector-image-backed file
- note whether the host bridge required any branch in bridge logic

## Required Output

The first bridge attempt should produce a short report with:

- which open mode was used
- which exchange mode was used
- whether D88 succeeded
- whether raw sector-image succeeded
- whether any request kind was missing
- whether any notification or timing semantic was unclear
- whether the bridge had to guess any controller behavior

## Non-Goals

The first bridge attempt should not:

- add new request kinds to the protocol first
- add write-capable controller behavior
- add fidelity-heavy MB8877 behavior
- turn the host bridge into a permanent daemon framework

If the proof fails, the bridge should document the smallest missing piece and stop there.
