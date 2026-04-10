# Gemini Task Report

## Task ID
20260321-140734-m13-xdos-dir-byte-placement-correlation-retry2

## Instruction Filename
20260321-140734-m13-xdos-dir-byte-placement-correlation-retry2.md

## Branch Name
codex/m13-xdos-dir-byte-placement-correlation-retry2

## Summary
Successfully retried the M13 directory-byte analysis with the narrowest defensible target. By correlating directory entries with physical sector content on `XDOS_SYS.D88` and `XDOSUTIL.D88`, it was proven that directory offsets `0x1B/0x1C` (Pair A) match the starting Track and Sector of file content. Offsets `0x1D/0x1E` (Pair B) were identified as parameters consumed by the `helper_d6af` read engine, although their physical semantics are inconsistent (e.g., in `X1-BIOS`). All unproven terminology ("FAM", "physical", "cluster") has been removed or downgraded as requested.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added "Evidence-Graded" correlation table and revised interpretation section.
- `analysis/xdos-kernel/README.md`: Updated "Critical Unknowns" with neutral and defensible wording.

## Commands
```bash
git checkout -b codex/m13-xdos-dir-byte-placement-correlation-retry2 develop
# [Analysis via od and grep]
git add analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md
git commit -m "docs(xdos): evidence-graded directory byte correlation for track/sector"
```

## Evidence
- **Strong Defensible Correlation (Pair A: 0x1B/0x1C)**:
    - `X-DOS System`: `0x1B=02, 0x1C=01` -> Starts at Track 2, Sector 1.
    - `X1-BIOS`: `0x1B=06, 0x1C=03` -> Starts at Track 6, Sector 3.
    - `SX-BASIC`: `0x1B=06, 0x1C=08` -> Starts at Track 6, Sector 8.
    - `GAME LOAD.DOC`: `0x1B=09, 0x1C=03` -> Starts at Track 9, Sector 3.
    - `MML.DOC`: `0x1B=0a, 0x1C=02` -> Starts at Track 10, Sector 2.
- **Engine Parameter Observation (Pair B: 0x1D/0x1E)**:
    - Assembly logic in `helper_d6af` (at `0xD6AF`) loads `D` from `0x1D` and `E` from `0x1E` before proceeding with the read.
    - In `X1-BIOS`, Pair B is `0x01 / 0x03`, which does not match the physical start (`Track 6, Sector 3`), proving Pair B is not a direct physical address.

## Risks
- **Traversal Logic**: The exact bit-level semantics of Pair B inside the traversal engine (likely involving logical records or drive selection) remain unknown.
- **Fragmented Files**: It is unproven if Pair A/B are sufficient for non-sequential files or if a separate allocation map is required.

## Requested Review
Review the updated "Evidence-Graded" section in `boot_and_io_notes.md` to ensure the terminology is strictly defensible for future implementation phases.

## Contradictions
The previous failed reports misidentified the offsets (using 0x1A/0x1B or over-naming them). This retry corrected the offsets to `0x1B/0x1C` (Pair A) and `0x1D/0x1E` (Pair B) based on direct binary observation.

## Provisional Conclusions
- **Offsets 0x1B/0x1C (Pair A)**: Observed content-start pair (Track/Sector).
- **Offsets 0x1D/0x1E (Pair B)**: Directory bytes consumed by `helper_d6af` (engine parameters).

## Unknown
- The meaning of offset `0x1A` and offset `0x1F`.
- The exact mechanism for continuing a file read after the first sector.

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **Intentional Downgrades**: The terms "FAM", "physical", "cluster", "drive", and "side" were intentionally avoided for directory fields because they are not yet proven. Neutral terms like "observed content-start pair" were used instead.
