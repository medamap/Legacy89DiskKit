# Gemini Work Report

## Task ID
20260321-152321-m13c-xdos-engine-pair-vs-placement-retry2

## Instruction
20260321-152321-m13c-xdos-engine-pair-vs-placement-retry2.md

## Branch Name
codex/m13c-xdos-engine-pair-vs-placement-retry2

## Summary
Investigated whether the `0x1D/0x1E` pair exactly matches the initial observed placement pair. A custom python script was used to read D88 headers (C, H, R) to identify the starting points of several files across `XDOS_SYS.D88` and `XDOSUTIL.D88`. The script confirmed an exact match: for all sampled files, computing the observed placement pair as `(Header C * 2 + Header H, Header R)` perfectly aligns with the `0x1D/0x1E` directory values.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

## Commands
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_placement_temp.py`
- `git commit -m "Update analysis of 1D/1E pair and observed placement correlation"`

## Evidence
`boot_and_io_notes.md` was updated with the following raw observation snippets from the script output that justify the evidence table rows:

```text
=== XDOS_SYS.D88 ===
File: X-DOS System    | 1D/1E: 02/01 | Target Header: C=01 H=0 R=01 | Bytes: 02 02 09 03 01 0A 04 01
File: X-DOS System X1 | 1D/1E: 04/02 | Target Header: C=02 H=0 R=02 | Bytes: 04 03 08 05 01 0A 06 01
File: SX-BASIC        | 1D/1E: 06/08 | Target Header: C=03 H=0 R=08 | Bytes: 06 09 02 07 01 0A 08 01
File: AUTO RUN.BAS    | 1D/1E: 42/04 | Target Header: C=33 H=0 R=04 | Bytes: 42 05 01 00 00 00 00 00
=== XDOSUTIL.D88 ===
File: X-DOS System    | 1D/1E: 02/01 | Target Header: C=01 H=0 R=01 | Bytes: 02 02 09 03 01 0A 04 01
File: SX-BASIC        | 1D/1E: 04/02 | Target Header: C=02 H=0 R=02 | Bytes: 04 03 08 05 01 0A 06 01
File: AUTO RUN.BAS    | 1D/1E: 06/04 | Target Header: C=03 H=0 R=04 | Bytes: 06 05 01 00 00 00 00 00
File: Overlay module  | 1D/1E: 06/06 | Target Header: C=03 H=0 R=06 | Bytes: 06 07 04 07 01 0A 08 01
```
The exact correlation `1D = C*2+H` and `1E = R` is clearly demonstrated.

## Risks
- The method demonstrates how the file start is located but how it continues mapping across subsequent spaces remains unknown.

## Requested Review
- Please review the changes in `boot_and_io_notes.md` and `README.md` verifying the updated evidence-graded section on the `0x1D/0x1E` pair versus observed placement pair correlation.

## Contradictions
- None.

## Provisional Conclusions
- The `0x1D/0x1E` pair exactly matches the initial observed placement pair for all sampled files.

## Unknown
- How the system resolves subsequent portions of files once the initial observed placement pair is consumed remains unknown.

## Context and Clean Up Constraints Note
- Explicitly confirming that unrelated local changes were not reset, stashed, reverted, or cleaned up. Unrelated untracked files and modifications remain unchanged.

## Vocabulary Constraints Note
- Explicitly avoided using the following prohibited semantic labels: FAM, cluster, physical, logical, drive, side, load address, entry point.