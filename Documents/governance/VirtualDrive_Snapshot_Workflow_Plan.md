# VirtualDrive Snapshot Workflow Plan

## Intent

The first practical VirtualDrive workflow should not depend on emulator-side integration.

Instead, L89 should support:

- mounting a disk image into an in-memory virtual session
- editing files from the host side
- exporting a point-in-time snapshot of that mounted session as a new disk image

This allows non-L89-aware emulators to use the latest edited disk state without requiring direct session sharing.

## Primary Workflow

Example:

```text
l89 disk mount a.d88 z:
# cross development, file copy, replace, delete, etc.
l89 disk snapshot z:
```

Expected result:

- a new timestamped image is written, for example:
  - `a.202604040105235.d88`
- the emulator can then load the snapshot file directly

## Why Snapshot First

This is preferred over immediate emulator integration as the first slice because:

- most emulators are not L89-aware
- host-side development can already benefit from a mounted writable virtual session
- snapshot export avoids concurrent stale-state confusion between:
  - host-side in-memory mounted state
  - emulator-side in-memory loaded state
- the original image can remain untouched until the user explicitly flushes or saves

## Session Model

VirtualDrive should be based on an in-memory session model:

- open image
- load into a mutable session
- expose file-level operations to the host mount adapter
- keep the session dirty state in memory
- render to disk only on:
  - snapshot
  - flush/save
  - optional save-on-unmount policy

## Initial Scope

The recommended initial scope is:

- single-slot D88 only
- file-level read/write/delete/list only
- no random seek partial writes
- no direct multi-emulator live sharing
- snapshot support for non-L89-aware emulators

## Command Direction

Planned commands:

- `l89 disk mount <imagePath> <mountName>`
- `l89 disk unmount <mountName|imagePath>`
- `l89 disk list`
- `l89 disk snapshot <mountName|mountPoint>`

Possible future additions:

- `l89 disk flush <mountName|mountPoint>`
- `l89 disk save-as <mountName|mountPoint> <outputPath>`

## Save and Snapshot Semantics

The operations should be distinct:

- `snapshot`
  - render current mounted state to a new file
  - never overwrite the original image
- `flush`
  - write current mounted state back to the original image
- `unmount`
  - end the session
  - save/discard behavior should be policy-controlled

## Emulator Integration Roadmap

Future emulator integration remains valid, but it should be treated as a later phase:

1. Snapshot workflow for all emulators
2. Shared-session integration for L89-aware emulators
3. Optional direct live synchronization between emulator and mounted host session

This keeps the first implementation practical and low-risk.
