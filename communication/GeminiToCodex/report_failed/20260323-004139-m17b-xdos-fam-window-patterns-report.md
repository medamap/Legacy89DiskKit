# Gemini Implementation Report

## Task ID
20260323-004139-m17b-xdos-fam-window-patterns

## Instruction Filename
20260323-004139-m17b-xdos-fam-window-patterns.md

## Branch Name
codex/m17b-xdos-fam-window-patterns

## Summary
Completed the raw FAM window pattern analysis across `XDOS_SYS.D88` and `XDOSUTIL.D88`. Identified cross-disk and intra-disk byte relationships at the directory-indicated `0x1D` offsets in the FAM area (Track 2, Sector 1). Found that only the `X-DOS System` file maintains identical FAM window content across disks, while other files (like `SX-BASIC` and `Overlay module`) vary. Intra-disk analysis in `XDOS_SYS.D88` revealed repeated `00` motifs for many utility files.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added "Raw FAM Window Patterns (Analysis-Only)" section with comparison tables and observed motifs.
- `analysis/xdos-kernel/README.md`: Added a new critical unknown regarding FAM window pattern semantics.

## Commands
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/collect_raw_catalog.py`
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_fam.py`

## Evidence
- **Cross-Disk Same File (X-DOS System)**: 1D Offset `0x02` on both disks, FAM Window `09 03 01 0A 04 01 01 00` (Status: `same`).
- **Cross-Disk Same File (SX-BASIC)**: 1D Offset `0x06` (SYS) vs `0x04` (UTIL), FAM Windows `04 01 01 00 00 00 00 00` vs `01 0A 04 01 01 00 00 00` (Status: `different`).
- **Intra-Disk Repeated Motif (XDOS_SYS)**: Multiple files starting from `Overlay module` (`0x09`) through many utility entries share a `00 00 00 00 00 00 00 00` FAM window.
- **Direct Match (XDOS_SYS)**: `X1-BIOS` and `SX-BASIC` both share 1D offset `0x06` and FAM window `04 01 01 00 00 00 00 00` (Status: `same`).

## Risks
- The relationship between the directory's `0x1D` byte and the FAM offset is empirically strong but not bit-level proven.
- Files sharing the same 1D offset and FAM window (like `X1-BIOS` and `SX-BASIC`) suggest potential shared-cluster or multi-track allocation logic that is not yet decoded.

## Requested Review
- Verify the classification of `same`/`different`/`repeated` in `boot_and_io_notes.md`.
- Confirm if the new critical unknown in `README.md` correctly captures the current boundary of analysis.

## Contradictions
- None identified. The raw data is consistent across both images.

## Provisional Conclusions
- The `0x1D` byte in the directory entry reliably points to an offset in the FAM area (Track 2, Sector 1).
- The byte sequence starting at that offset is stable for core system files across disks but varies for higher-level utilities and user-added files.

## Unknown
- Bit-level semantics of the FAM window bytes.
- Traversal logic for files spanning multiple FAM entries.
- Relationship between the FAM window and physical sector allocation beyond the first sector.
