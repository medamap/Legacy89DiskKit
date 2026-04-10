# Gemini Work Report

## Task ID
20260321-153328-m13c-xdos-engine-pair-vs-placement-retry3

## Instruction
20260321-153328-m13c-xdos-engine-pair-vs-placement-retry3.md

## Branch Name
codex/m13c-xdos-engine-pair-vs-placement-retry3

## Summary
Retried M13c analysis by providing a fully tracked Python helper script (`analysis/xdos-kernel/verify_placement.py`). The script parses the directory to retrieve the `0x1D/0x1E` values for specific files and then scans the disk image to find the "candidate observed placement pair" using the formula `(C * 2 + H, R)`. The helper explicitly calculates and proves an exact match between the observed placement pair and the directory `0x1D/0x1E` pair for all sampled system files and utility files.

## Changed Files
- `analysis/xdos-kernel/verify_placement.py` (New tracked helper)
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

## Commands
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/verify_placement.py`
- `git checkout -b codex/m13c-xdos-engine-pair-vs-placement-retry3`
- `git add analysis/xdos-kernel/verify_placement.py analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
- `git commit -m "Update analysis: Prove 1D/1E pair correlates with observed placement pair"`

## Evidence
The tracked helper `verify_placement.py` provided the following raw output excerpt, explicitly showing the calculation used to justify an exact match:

```text
=== Verifying images/disk_org/x1/XDOS_SYS.D88 ===
File: X-DOS System X1
  Directory 1D/1E : 04/02
  Observed D88 Header : C=02, H=00, R=02
  Calculation : (C * 2 + H) = (02 * 2 + 00) = 04
  Calculation : (R) = 02 = 02
  Result : EXACT MATCH

File: SX-BASIC
  Directory 1D/1E : 06/08
  Observed D88 Header : C=03, H=00, R=08
  Calculation : (C * 2 + H) = (03 * 2 + 00) = 06
  Calculation : (R) = 08 = 08
  Result : EXACT MATCH

=== Verifying images/disk_org/x1/XDOSUTIL.D88 ===
File: Overlay module
  Directory 1D/1E : 06/06
  Observed D88 Header : C=03, H=00, R=06
  Calculation : (C * 2 + H) = (03 * 2 + 00) = 06
  Calculation : (R) = 06 = 06
  Result : EXACT MATCH
```

## Risks
- The script proves exact correlation only for the initial observed placement pair; how subsequent spaces are located remains unproven by this evidence.

## Requested Review
- Please verify the tracked helper script (`verify_placement.py`), its explicit calculation output, and the updated sections in `boot_and_io_notes.md` and `README.md`.

## Contradictions
- None.

## Provisional Conclusions
- For all sampled system files and utility files, the `0x1D/0x1E` directory pair is an exact match with the initial observed placement pair.

## Unknown
- How the system maps and allocates space subsequent to the initial `0x1D/0x1E` pair consumption remains unknown.

## Constraints Note
- **Clean Up**: Unrelated local changes were strictly preserved; untracked and modified files outside the target analysis files were not reset, stashed, reverted, or cleaned up.
- **Vocabulary Constraints**: Prohibited semantic labels (FAM, cluster, physical, logical, drive, side, load address, entry point) were intentionally avoided in the report, source code, and analysis notes updates.