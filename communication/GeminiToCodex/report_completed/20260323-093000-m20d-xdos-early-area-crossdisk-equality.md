# Gemini Implementation Report

## Task ID
20260323-093000-m20d-xdos-early-area-crossdisk-equality

## Instruction Filename
20260323-093000-m20d-xdos-early-area-crossdisk-equality.md

## Branch Name
`codex/m20d-xdos-early-area-crossdisk-equality`

## Summary
Classified the bit-for-bit equality of early-area regions across `XDOS_SYS.D88` and `XDOSUTIL.D88`. Added a new catalog section to `boot_and_io_notes.md` and updated `README.md`.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

## Commands
```bash
python3 compare_early_area.py  # Temporary script to compare sectors
git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md
```

## Evidence
Direct comparison of raw sectors (C, H, R) revealed:
- Track 0 Head 0 (R=1-16): all `same` (256B sectors).
- Track 0 Head 1 (R=1-6): `different` (512B sectors, includes FAT and Directory start).
- Track 0 Head 1 (R=7-10): `same` (512B sectors, end of Directory area).
- Track 1 Head 0 (R=1-10): all `same` (512B sectors, includes FAM and bdir).

The identity of the FAM sector (Track 1 Head 0 R=1) was re-confirmed.

## Risks
None. No semantics were assigned, only raw binary comparison results.

## Requested Review
Verify the newly added table in `boot_and_io_notes.md` correctly reflects the raw comparison findings.

## Contradictions
None observed. The findings align with existing notes about FAM identity.

## Provisional Conclusions
The binary mismatch in the early sectors of Track 0 Head 1 (R=1-6) is consistent with these being the FAT and active directory regions, which naturally differ between disks with different file contents. The identity of later directory sectors (R=7-10) suggest they might be unused or initialized to a common state.

## Unknown
The exact boundary where the directory area ends and the user data area begins remains unknown, although R=7-10 are documented as part of the directory area in existing notes.
