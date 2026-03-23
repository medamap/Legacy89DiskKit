# Gemini Task Report

## Task ID
20260321-140012-m13-xdos-dir-byte-placement-correlation-retry

## Instruction Filename
20260321-140012-m13-xdos-dir-byte-placement-correlation-retry.md

## Branch Name
codex/m13-xdos-dir-byte-placement-correlation-retry

## Summary
Successfully retried the M13 directory-byte analysis with strict evidence grading. By correlating directory entries with physical sector content on `XDOS_SYS.D88` and `XDOSUTIL.D88`, it was proven that directory offset 26 (`0x1A`) is the **Starting FAM Track** and offset 27 (`0x1B`) is the **Starting FAM Sector**. The previous claim that offsets 29 (`0x1D`) and 30 (`0x1E`) are physical Track/Sector was downgraded to "FAM Engine Parameters" due to observed mismatches (e.g., `X1-BIOS`), although they do correlate with starting positions in many data files.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Updated with an "Evidence-Graded" correlation table and revised interpretation.
- `analysis/xdos-kernel/README.md`: Updated "Critical Unknowns" to reflect the remaining ambiguity of bytes `0x1D/0x1E`.
- `analysis/xdos-kernel/read_path.asm`: Updated comments in `helper_d6af` to label offsets 29/30 as `FirstSectorR (?)` and `FirstSectorS (?)`.

## Commands
```bash
git checkout develop
git checkout -b codex/m13-xdos-dir-byte-placement-correlation-retry
# [Analysis via od and grep]
git add analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm
git commit -m "docs(xdos): evidence-graded directory byte correlation analysis"
```

## Evidence
- **FAM Track/Sector Correlation (Confirmed)**:
    - `X-DOS System`: `0x1A=02, 0x1B=01` -> Code found at Track 2, Sector 1.
    - `XEDIT`: `0x1A=0e, 0x1B=05` -> Code found at Track 14, Sector 5.
    - `GAME LOAD.DOC`: `0x1A=09, 0x1B=03` -> Data found at Track 9, Sector 3.
    - `MML.DOC`: `0x1A=0a, 0x1B=02` -> Data found at Track 10, Sector 2.
- **Ambiguity of 0x1D/0x1E (Downgraded)**:
    - `X1-BIOS`: `0x1D=03, 0x1E=01` but starts at `Track 6, Sector 1`. This mismatch proves `0x1D/0x1E` are not always direct physical track/sector.
- **Packing Support (Confirmed)**:
    - `MML.DOC` and `X...DOC` share Track 10 but start at different sectors (2 and 9), proving X-DOS supports multiple files per logical track (cluster) via directory-defined starting sectors.

## Risks
- **FAM Engine Logic**: The exact transformation from the `(Cluster, FirstSectorR)` inputs to physical FDC commands in `helper_d6af` remains partially unknown.
- **0x1D/0x1E Semantics**: While they usually point to the data start, the `X1-BIOS` exception suggests they might be logical offsets or Drive/Side parameters in some contexts.

## Requested Review
Review the updated "Revised Interpretation" in `boot_and_io_notes.md` to ensure the distinction between confirmed FAM pointers and ambiguous engine parameters is clear for subsequent implementation tasks.

## Contradictions
The previous report claimed `0x1D/0x1E` were the primary physical track/sector; this was contradicted by the `X1-BIOS` entry and has been corrected to show that `0x1A/0x1B` are the confirmed starting FAM pointers.

## Provisional Conclusions
- **Offset 26 (`0x1A`)**: Starting FAM Track (Cluster).
- **Offset 27 (`0x1B`)**: Starting FAM Sector.
- **Offset 29/30 (`0x1D/0x1E`)**: Inputs to the FAM engine, often correlating with the first data sector, but semantics are non-physical in some cases.

## Unknown
- The bit-level structure of the FAM list pointed to by `0x1A/0x1B`.
- The exact role of the FAM engine in resolving `0x1D/0x1E`.

## Explicit Notes
- **Unrelated local changes were not reset or cleaned.**
- **Downgrade Note**: Claims about `0x1D/0x1E` being physical track/sector were downgraded to "FAM Engine Parameters" because `X1-BIOS` showed a physical mismatch (`0x1D=03` but data at `Sector 1`), proving they are not always direct addresses.
