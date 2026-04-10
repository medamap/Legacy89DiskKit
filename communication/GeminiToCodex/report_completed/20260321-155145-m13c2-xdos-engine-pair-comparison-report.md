# Gemini Work Report

## Task ID
20260321-155145-m13c2-xdos-engine-pair-comparison

## Instruction
20260321-155145-m13c2-xdos-engine-pair-comparison.md

## Branch Name
codex/m13c2-xdos-engine-pair-comparison

## Summary
Compared the independently detected observed placement pair (using `find_file_start.py`) against the directory `0x1D/0x1E` pair for the sampled files across `XDOS_SYS.D88` and `XDOSUTIL.D88`. We discovered an exact match for all sampled files. Added an evidence-graded comparison section to `boot_and_io_notes.md` reflecting this exact match and updated `README.md` to materially improve the critical-unknown wording based on these findings.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

## Commands
- `git checkout develop && git pull origin develop && git checkout -b codex/m13c2-xdos-engine-pair-comparison`
- `python3 analysis/xdos-kernel/find_file_start.py`
- (python script to parse directory `0x1D` and `0x1E` fields)
- `git add analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
- `git commit -m "Update analysis: Compare directory 0x1D/0x1E pair with observed placement pair"`

## Evidence
The raw observed placement pair from the helper script matches the `0x1D/0x1E` bytes extracted from the directory entry for each file exactly:

| Disk | Filename | `0x1D/0x1E` Pair | Observed Placement Pair | Match Status |
| :--- | :--- | :--- | :--- | :--- |
| `XDOS_SYS.D88` | `X-DOS System X1` | `(04, 02)` | `(04, 02)` | exact match |
| `XDOS_SYS.D88` | `SX-BASIC` | `(06, 08)` | `(06, 08)` | exact match |
| `XDOSUTIL.D88` | `Overlay module` | `(06, 06)` | `(06, 06)` | exact match |

## Risks
None.

## Requested Review
Review the updated conclusions in `boot_and_io_notes.md` and the refined wording of the Critical Unknowns in `README.md`.

## Contradictions
There are no contradictions across system files and utility files; the exact match consistency holds true across both disks for all sampled files.

## Provisional Conclusions
The `0x1D/0x1E` pair equals the independently detected observed placement pair for all sampled files without any deviations.

## Unknown
The explicit downstream translation of these values within the deeper FDC read logic (e.g. `helper_d6af` traversal) remains unknown.

## Explicit Raw Observation Snippets
Helper output:
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

## Explicit Note on Local Changes
Unrelated local changes were strictly preserved; no resets, stashes, reverts, or untracked cleans were performed.

## Explicit Note on Prohibited Semantic Labels
The prohibited words ("FAM", "cluster", "physical", "logical", "drive", "side", "load address", "entry point") were intentionally avoided. The term "observed placement pair" was used instead.