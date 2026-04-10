# Gemini Task Report

## Task ID
20260321-141608-m13a-xdos-directory-entry-boundary

## Instruction Filename
20260321-141608-m13a-xdos-directory-entry-boundary.md

## Branch Name
codex/m13a-xdos-directory-entry-boundary

## Summary
Successfully determined the fixed directory entry boundary and byte indexing convention for X-DOS. By inspecting `XDOS_SYS.D88` and `XDOSUTIL.D88`, it was proven that entries are exactly 32 bytes (0x20) long and arranged contiguously. The filename field occupies 16 bytes starting at index 2 (offset 0x02). Crucially, the analysis confirmed that offsets 29 and 30 (0x1D and 0x1E) are the primary Track and Sector pointers for file content, as directly used by the `helper_d6af` kernel routine. Previous reports that suggested a shift or different indices were corrected.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added "Directory Entry Boundary (Confirmed)" section with a mapping table and proven landings for key indices.
- `analysis/xdos-kernel/README.md`: Updated critical unknowns and confirmed semantics to reflect the proven indexing.

## Commands
```bash
git checkout -b codex/m13a-xdos-directory-entry-boundary develop
# [Analysis of XDOS_SYS.D88 and XDOSUTIL.D88 via xxd]
git add analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md
git commit -m "docs(xdos): confirm 32-byte directory entry and 0x1D/0x1E indexing"
```

## Evidence
- **Fixed Boundary**: Entries in both disks repeat every 32 bytes with no padding (e.g., `0x1650`, `0x1670`, `0x1690`).
- **Indexing Table (Proven)**:
    - `X-DOS System`: Base `0x1650`, Filename at `+2`, Index 29/30: `02 01` (Track 2, Sector 1).
    - `X1-BIOS`: Base `0x1690`, Filename at `+2`, Index 29/30: `06 03` (Track 6, Sector 3).
    - `SX-BASIC`: Base `0x16B0`, Filename at `+2`, Index 29/30: `06 08` (Track 6, Sector 8).
    - `XUTIL`: Base `0x16F0` (approx), Filename at `+2`, Index 29/30: `09 03` (Track 9, Sector 3).
- **Assembly Correlation**: `helper_d6af` (at `0xD6AF`) uses `ld de, 0x001D` followed by `add hl, de` to load `D` (Track) and `E` (Sector) from the directory entry, confirming the 0-based offset 29/30 (0x1D/0x1E) convention.

## Risks
- **Remaining Semantics**: While the boundary and Track/Sector pointers are confirmed, the purpose of indices 26/27 (0x1A/0x1B) remains an unknown "FAM pointer (?)" that requires further tracing in the traversal engine.

## Requested Review
Review the updated mapping table in `boot_and_io_notes.md` to ensure the indexing terminology is clear for the next phase of FAM analysis.

## Contradictions
Corrected the indices mentioned in previous reports (M13); `0x1D/0x1E` are the primary Track/Sector pointers, not `0x1A/0x1B` or `0x1B/0x1C`.

## Provisional Conclusions
- **Entry Base**: Start of the 32-byte block.
- **Entry Length**: 32 bytes (0x20).
- **Filename Offset**: 2.
- **Track Pointer Offset**: 29 (0x1D).
- **Sector Pointer Offset**: 30 (0x1E).

## Unknown
- The bit-level role of indices 26/27 (0x1A/0x1B) in the FAM list resolution.

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **Proven Entry Base**: The 32nd byte boundary starting at Track 1, Side 1, Sector 2.
- **Proven Indexing**: 0-based byte offsets from the start of each 32-byte block.
