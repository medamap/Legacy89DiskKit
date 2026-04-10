# Gemini Report

## Task ID
20260323-000732-m17a-xdos-fam-raw-byte-catalog

## Instruction Filename
20260323-000732-m17a-xdos-fam-raw-byte-catalog.md

## Branch Name
`codex/m17a-xdos-fam-raw-byte-catalog`

## Summary
I have built a primary-evidence catalog for X-DOS files by sampling representative files across `XDOS_SYS.D88` and `XDOSUTIL.D88`. I collected directory bytes `0x1A` through `0x1E`, verified that the `0x1D/0x1E` pair matches the observed physical placement pair `(C*2+H, R)`, and recorded raw FAM-area bytes associated with the starting track of each file. The observations are documented in `analysis/xdos-kernel/boot_and_io_notes.md`.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/dump_dir_entries.py` (added)
- `analysis/xdos-kernel/collect_raw_catalog.py` (added)
- `analysis/xdos-kernel/dump_fam.py` (added)

## Commands
- `python3 analysis/xdos-kernel/dump_dir_entries.py`
- `python3 analysis/xdos-kernel/collect_raw_catalog.py`
- `python3 analysis/xdos-kernel/dump_fam.py`
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_file_start.py --help`

## Evidence
The following raw data was extracted and recorded in `analysis/xdos-kernel/boot_and_io_notes.md`:

### Sampled Files (XDOS_SYS.D88)
- `X-DOS System`: 1D/1E=(02, 01), Placement=(02, 01), FAM[02]=02
- `SX-BASIC`: 1D/1E=(06, 08), Placement=(06, 08), FAM[06]=04
- `Overlay module`: 1D/1E=(09, 02), Placement=(09, 02), FAM[09]=00
- `XEDIT`: 1D/1E=(10, 01), Placement=(10, 01), FAM[10]=00

### Sampled Files (XDOSUTIL.D88)
- `X-DOS System`: 1D/1E=(02, 01), Placement=(02, 01), FAM[02]=02
- `SX-BASIC`: 1D/1E=(04, 02), Placement=(04, 02), FAM[04]=01
- `Overlay module`: 1D/1E=(06, 06), Placement=(06, 06), FAM[06]=04
- `AUTO RUN.BAS`: 1D/1E=(06, 04), Placement=(06, 04), FAM[06]=04

### Observations
- Directory `0x1D/0x1E` exactly matches observed physical placement `(C*2+H, R)`.
- Directory `0x1B/0x1C` is stable across disks for identical files (e.g., `SX-BASIC` is `67 80`).
- FAM bytes at the track ID index frequently contain another track ID, suggesting a simple track-based allocation chain for these system files.

## Risks
- No semantics were assigned to the bytes as per the instruction, so the meaning of FAM bytes other than the first few remains unknown.
- The packing logic for the 512-byte FAM sector is still not proven.

## Requested Review
- Verify the mapping of `(C*2+H, R)` against the provided directory entries and FAM bytes.
- Review the newly added helper scripts for clarity and reproducibility.

## Contradictions
None observed. The mapping rule remains consistent across all sampled files and both disks.

## Provisional Conclusions
- The `0x1D/0x1E` pair is the definitive entry point for the file payload's physical location.
- The FAM area at Track 2, Sector 1 stores track-level allocation chains, at least for the beginning of the disk.

## Unknown
- Role of directory index `0x1A`.
- Exact packing of the FAM sector.
- Directory field representing total file length.
