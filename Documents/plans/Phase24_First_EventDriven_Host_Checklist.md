# Phase 24 First Event-Driven Host Checklist

## Goal

This checklist defines the minimum proof for the first real event-driven emulator host bridge.

The bridge should prove that the existing host-facing protocol is already sufficient for a read-only controller-facing integration.

## Preconditions

- the host can launch `Legacy89DiskKit.Cli host stdio`
- the host can exchange line-delimited JSON with the process
- the host can issue `QueryCapabilities` before any controller request
- the host can choose either `OpenDiskPath` or `OpenDiskImage`

## Required Requests

The first proof should stay inside this request set:

- `QueryCapabilities`
- `OpenDiskPath` or `OpenDiskImage`
- `CloseDisk`
- `SelectDrive`
- `SelectSide`
- `Reset`
- `WriteRegister`
- `ReadRegister`
- `Advance`
- `QueryState`

No host-specific request should be added before the checklist is attempted once.

## D88-Backed Read-Only Proof

The first host proof should verify:

1. capability handshake succeeds
2. a D88-backed image can be opened
3. the selected drive can be set explicitly
4. track and sector registers can be written
5. a read-sector command can be issued
6. the host can observe a pending advance request or a busy state
7. the host can advance time externally
8. the host can observe IRQ and DRQ after completion
9. the host can read data bytes through the data register
10. the host can close the image cleanly

## Raw Sector-Image Follow-Up

After the D88-backed proof succeeds, the same host bridge should verify:

1. a raw sector-image-backed medium can be opened
2. the same request flow still works
3. no host-specific protocol fork is needed

## Required Observations

The first host proof should record:

- which request sequence the host used
- whether path-open or buffer-open was used
- whether plain response or notification-aware exchange was used
- what timing-driving shape the host used
- whether any expected signal was missing
- whether any status outcome was ambiguous for the host

## Escalation Rule

If the host cannot complete this checklist, the proof should record the smallest missing capability first.

Preferred order for identifying gaps:

1. missing notification or signal semantics
2. missing timing-driving semantics
3. missing request kind
4. host-specific lifecycle mismatch

This keeps Phase 24 evidence-driven rather than speculative.
