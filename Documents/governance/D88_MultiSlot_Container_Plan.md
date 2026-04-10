# D88 Multi-Slot Container Plan

## Current Safety Policy

Current CLI behavior is intentionally conservative for multi-slot D88 containers.

- Read operations target only the first slot.
- Write operations against an existing multi-slot D88 container are rejected.
- Users should first split the container into a single-slot D88 image, then retry the write operation.

This policy avoids accidental corruption while the product still treats `D88` mostly as a single-disk workflow.

## Recommended Architecture

The long-term design should treat a `D88` file as a container that owns multiple disk slots.

- `D88Container`
  - owns `Slots`
  - renders the complete file
- `DiskSlot`
  - owns header, geometry, and sector payloads for one disk

All edits should apply to an in-memory container model, not directly to on-disk byte offsets.

## Why In-Place Writes Are Unsafe

Slot bodies in a D88 file are variable-sized.

- formatting a slot can change geometry
- different sector sizes change total disk size
- inserting or deleting a slot shifts all following offsets

Because of that, in-place seek writes are not a safe default for multi-slot D88 editing.

## Save Strategy Recommendation

Recommended save strategy:

- read the container into memory
- modify the in-memory slot model
- render the entire container when saving

Recommended policies:

- CLI: save on command completion
- Managed API: explicit `Save()` / `Flush()` by default
- all save flows: atomic replace through a temporary output file

Optional safety policy:

- versioned backup before replace
- example: `sample.d88` -> `sample.backup.0001.d88`

## Future Work

When multi-slot D88 write support is added, the product should support:

- slot selection
- slot insertion
- slot deletion
- slot extraction to single-slot D88
- single-slot D88 import into a chosen slot
- full container re-render on save

Until that work is implemented, rejecting write operations is the correct safety behavior.
