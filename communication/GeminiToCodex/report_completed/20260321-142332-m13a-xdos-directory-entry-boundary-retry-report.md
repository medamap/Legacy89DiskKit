# Gemini Task Report

## Task ID
20260321-142332-m13a-xdos-directory-entry-boundary-retry

## Instruction Filename
20260321-142332-m13a-xdos-directory-entry-boundary-retry.md

## Branch Name
codex/m13a-xdos-directory-entry-boundary-retry

## Summary
Successfully proved the raw directory entry boundary, fixed length, and filename byte span. By inspecting `XDOS_SYS.D88` and `XDOSUTIL.D88`, it was confirmed that entries are exactly 32 bytes (0x20) long and contiguous. The filename starts at index 2 (offset +2) and spans 16 bytes. The physical locations of indices 0x1A, 0x1B, 0x1D, and 0x1E were identified for consecutive entries, with no field semantics assigned in this phase.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added "Directory Entry Boundary (Confirmed)" section with a raw evidence table and boundary summary.
- `analysis/xdos-kernel/README.md`: Updated "Critical Unknowns" to reflect that directory field semantics are still open.

## Commands
```bash
git checkout -b codex/m13a-xdos-directory-entry-boundary-retry develop
# [Analysis of XDOS_SYS.D88 and XDOSUTIL.D88 via xxd]
git add analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md
git commit -m "docs(xdos): confirm directory entry boundary and raw indexing"
```

## Evidence
- **Fixed Boundary**: Entries repeat every 32 bytes (e.g., `0x1650`, `0x1670`, `0x1690`).
- **Raw Evidence Table (XDOS_SYS.D88)**:
    - Entry 1: Base `0x1650`, Filename at `+2`, Index 0x1A/0x1B: `BA/D5`, Index 0x1D/0x1E: `02/01`.
    - Entry 2: Base `0x1670`, Filename at `+2`, Index 0x1A/0x1B: `12/A8`, Index 0x1D/0x1E: `04/02`.
    - Entry 3: Base `0x1690`, Filename at `+2`, Index 0x1A/0x1B: `12/B4`, Index 0x1D/0x1E: `06/03`.
- **Filename Span**: Bytes 2 through 17 (16 bytes) in both disks.

## Risks
- **Traversal Logic**: While the indices are located, their roles in FAM/Sector traversal remain unproven and require further kernel analysis.

## Requested Review
Review the raw evidence table in `boot_and_io_notes.md` to ensure the byte landings are correctly recorded for the next analysis phase.

## Contradictions
None. This retry strictly followed the boundary-only mandate.

## Provisional Conclusions
- **Entry Base**: Start of any 32nd byte boundary in the directory area (e.g., Track 1, Sector 2).
- **Entry Length**: 32 bytes (0x20).
- **Filename Offset**: 2.
- **Index 0x1A/0x1B Offset**: 26/27.
- **Index 0x1D/0x1E Offset**: 29/30.

## Unknown
- The specific bit-level semantics of the data stored at indices 0x1A, 0x1B, 0x1D, and 0x1E.

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **No field semantics were assigned in this retry.**
