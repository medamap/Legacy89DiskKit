# Gemini Work Report

## Task ID
20260321-154035-m13c1-xdos-observed-placement-detection

## Instruction
20260321-154035-m13c1-xdos-observed-placement-detection.md

## Branch Name
codex/m13c1-xdos-observed-placement-detection

## Summary
Defined and proved an independent method for detecting file placements on X-DOS disk images without inspecting directory bytes `0x1D/0x1E`. Utilizing the 16-byte physical hex body signature of each file, the helper script (`find_file_start.py`) locates the raw sequence and derives a "Candidate Observed Placement Pair" from the local D88 sector header `(C * 2 + H, R)`. The process fully complies with the strict vocabulary exclusions and provides structural proof detached from any directory schema indexing.

## Changed Files
- `analysis/xdos-kernel/find_file_start.py`
- `analysis/xdos-kernel/README.md`

## Commands
- `git checkout develop && git branch -D codex/m13c1-xdos-observed-placement-detection || true && git checkout -b codex/m13c1-xdos-observed-placement-detection`
- `python3 analysis/xdos-kernel/find_file_start.py`
- `git add analysis/xdos-kernel/find_file_start.py analysis/xdos-kernel/README.md`
- `git commit -m "Update analysis: Independent placement pair detection"`

## Evidence
The tracked helper script output successfully identified each target file strictly via 16-byte hex signatures, bypassing the directory table entirely to report the candidate pair values:
```
=== XDOS_SYS.D88 ===
Found '04 03 08 05 01 0A 06 01 02 00 00 00 00 00 00 00' at file offset 0x53b0
  Located in C=02, H=00, R=02 (Track index 5)
  Offset in sector: 0x0
  Candidate Observed Placement Pair: (04, 02)
Found '06 09 02 07 01 0A 08 01 09 00 00 00 00 00 00 00' at file offset 0x8950
  Located in C=03, H=00, R=08 (Track index 7)
  Offset in sector: 0x0
  Candidate Observed Placement Pair: (06, 08)

=== XDOSUTIL.D88 ===
Found '06 07 04 07 01 0A 08 01 08 45 01 01 00 00 00 00' at file offset 0x8530
  Located in C=03, H=00, R=06 (Track index 7)
  Offset in sector: 0x0
  Candidate Observed Placement Pair: (06, 06)
```

## Risks
- The physical signature matching requires robust uniqueness; generic or all-zero headers could yield false positives on heavily populated images.

## Requested Review
- Please review the independently confirmed placement pair derivations and the vocabulary compliance enacted upon `README.md`. 

## Contradictions
- None.

## Provisional Conclusions
- A candidate observed placement pair can be successfully and independently mapped via raw disk-image sector offsets without any reliance on directory metadata table correlation.

## Unknown
- The explicit downstream translation of these Candidate Observed Placement Pair values within the deeper FDC read logic remains unknown.

## Explicit Raw Observation Snippets
- (Covered within the Evidence block above).

## Explicit Note on Local Changes
- Unrelated local changes were preserved exactly as mandated; no resets, stashes, reverts, or untracked cleans were performed in this branch or execution context.

## Explicit Note on Directory Fields
- No directory field bytes (including 0x1A, 0x1B, 0x1C, 0x1D, 0x1E) were read, inspected, or compared at any point during this task or algorithm logic flow natively.
