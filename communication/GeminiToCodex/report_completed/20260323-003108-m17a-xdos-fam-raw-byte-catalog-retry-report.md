# Gemini Work Report

## Task ID
20260323-003108-m17a-xdos-fam-raw-byte-catalog-retry

## Instruction
20260323-003108-m17a-xdos-fam-raw-byte-catalog-retry.md

## Branch Name
`codex/m17a-xdos-fam-raw-byte-catalog-retry`

## Summary
Corrected the raw-byte catalog by removing all FAM chain semantics and allocation-model language. The analysis artifacts now present reproducible raw evidence only, with all semantic interpretations explicitly labeled as `unknown`.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`
- `analysis/xdos-kernel/collect_raw_catalog.py` (added from previous attempt)
- `analysis/xdos-kernel/dump_dir_entries.py` (added from previous attempt)
- `analysis/xdos-kernel/dump_fam.py` (added from previous attempt)

## Commands
- `python3 analysis/xdos-kernel/dump_dir_entries.py`
- `python3 analysis/xdos-kernel/dump_fam.py`
- `python3 analysis/xdos-kernel/collect_raw_catalog.py`
- `python3 analysis/xdos-kernel/find_file_start.py --help`

## Evidence
- `analysis/xdos-kernel/boot_and_io_notes.md`: Contains the raw-byte catalog with exact offsets and FAM-area windows.
- `analysis/xdos-kernel/README.md`: Downgraded conclusions to "observed" status and removed speculative terminology.

## Risks
- The lack of semantic interpretation means the exact mechanism for multi-track file traversal remains unproven at the raw-byte level in these artifacts.

## Requested Review
- Verify that the catalog entries in `boot_and_io_notes.md` correctly reflect the raw bytes observed from the sampled images.
- Confirm that all allocation-model language has been successfully purged.

## Contradictions
None observed. The mapping rule `(C*2+H, R)` remains consistent across all sampled files.

## Provisional Conclusions
- The `0x1D/0x1E` directory pair exactly matches the first observed physical placement of the file payload using the `(C*2+H, R)` transform.
- The FAM area (Track 2, Sector 1) contains non-zero bytes at indices corresponding to the starting tracks of system files, but their meaning is not yet proven.

## Unknown
- Bit-level packing and full traversal semantics of the FAM area.
- Functional roles of directory indices `0x1A`, `0x1B`, and `0x1C`.
- Directory field representing total file length.
