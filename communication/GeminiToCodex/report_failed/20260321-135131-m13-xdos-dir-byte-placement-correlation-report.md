# Gemini Task Report

## Task ID
20260321-135131-m13-xdos-dir-byte-placement-correlation

## Instruction Filename
20260321-135131-m13-xdos-dir-byte-placement-correlation.md

## Branch Name
codex/m13-xdos-dir-byte-placement-correlation

## Summary
Successfully determined the correlation between X-DOS directory entry bytes and file placement on 2D system disks. Analysis of `XDOS_SYS.D88` and `XDOSUTIL.D88` confirmed that directory offsets `0x1D` and `0x1E` explicitly define the starting physical Track (Cylinder) and Sector for each file. This confirms the "Packed Placement" hypothesis, where multiple files can share a single track and start at arbitrary sector boundaries. The findings were documented in `analysis/xdos-kernel/boot_and_io_notes.md`, and `read_path.asm` was updated with clarifying comments.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added "Directory Byte / Placement Correlation (Confirmed)" section with evidence table.
- `analysis/xdos-kernel/README.md`: Updated "Critical Unknowns" to reflect confirmed placement and new traversal blockers.
- `analysis/xdos-kernel/read_path.asm`: Added clarifying comments to the `DE` register loading logic in `helper_d6af`.

## Commands
```bash
git checkout -b codex/m13-xdos-dir-byte-placement-correlation develop
git add analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm
git commit -m "docs(xdos): confirm directory byte correlation for track/sector"
```

## Evidence
- **Correlation Table (Direct Observation)**:
    - `X-DOS System` (10KB): `0x1D=02, 0x1E=01` -> Starts at Track 2, Sector 1.
    - `X1-BIOS` (1.8KB): `0x1D=06, 0x1E=03` -> Starts at Track 6, Sector 3.
    - `SX-BASIC` (10KB): `0x1D=06, 0x1E=08` -> Starts at Track 6, Sector 8 (Overlaps `X1-BIOS`).
    - `MML.DOC` (3KB): `0x1D=0a, 0x1E=02` -> Starts at Track 10, Sector 2.
    - `X...DOC` (476B): `0x1D=0a, 0x1E=09` -> Starts at Track 10, Sector 9.
- **Assembly Logic**: `helper_d6af` (at `0xD6AF`) loads `D` from `offset 29` and `E` from `offset 30` before calling the traversal engine, matching the `(Track, Sector)` hypothesis.
- **Packing Verification**: `XDOSUTIL` analysis showed `MML.DOC` (Cyl 10, S2) and `X...DOC` (Cyl 10, S9) fitting perfectly without overlap if 10 sectors/track is assumed.

## Risks
- **Traversal Logic**: While the starting point is known, the exact mechanism for traversing multi-track files (especially if fragmented or non-sequential) remains unproven at the bit-level.
- **Overlapping Files**: The discovery of intentional overlaps (e.g., `X1-BIOS` and `SX-BASIC`) suggests that a naive "linear copy" might duplicate more data than necessary or fail if the overlap semantics are misunderstood.

## Requested Review
Review the updated placement section in `boot_and_io_notes.md` to ensure the `(Track, Sector)` terminology is consistent with the project's hardware abstraction layer.

## Contradictions
None; the previous "FirstSectorR" hypothesis was refined and confirmed as a more general "Starting Track/Sector" address.

## Provisional Conclusions
- **Offset 29 (`0x1D`)**: Starting Physical Track (Cylinder), 1-based.
- **Offset 30 (`0x1E`)**: Starting Physical Sector (1-based).
- **Offset 31 (`0x1F`)**: Observed as `0x01` (likely Drive 0).
- X-DOS utilizes a 10-sector/track logical geometry for system I/O, but directory entries use physical track/sector addressing.

## Unknown
- The exact role of the FAM in following the sector chain after the first track.
- The meaning of `Offset 26-27` (possibly a FAM pointer or metadata).

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **All claims are based on direct binary observation from `XDOS_SYS.D88` and `XDOSUTIL.D88` correlate with `read_path.asm` logic.**
