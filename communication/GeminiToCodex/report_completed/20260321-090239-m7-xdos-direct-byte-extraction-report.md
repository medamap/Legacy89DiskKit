# Gemini Task Report

## Task ID
20260321-090239-m7-xdos-direct-byte-extraction

## Instruction Filename
20260321-090239-m7-xdos-direct-byte-extraction.md

## Branch Name
codex/m7-xdos-direct-byte-extraction

## Summary
Successfully identified and extracted the direct byte window for the X-DOS syscall jump table from the `XDOS_SYS.D88` disk image. The jump table was found at physical Track 6, Sector 1 (D88 offset `0x7c13`), corresponding to the memory region around `0xED78`. Updated the analysis workspace with the observed `C3 xx yy` (jp) instructions and synchronized `labels.tsv` with the newly confirmed implementation targets.

## Changed Files
- `analysis/xdos-kernel/read_path.asm`: Updated syscall jump table with observed bytes.
- `analysis/xdos-kernel/labels.tsv`: Added implementation labels for syscall jump targets and updated sources.
- `analysis/xdos-kernel/boot_and_io_notes.md`: Added detailed observation notes for the syscall jump table and identified extraction limits between disks.

## Commands
```bash
# Search for syscall jump pattern
od -An -v -tx1 images/disk_org/x1/XDOS_SYS.D88 | tr -d ' \n' | grep -o "c3..c8c3..c8"
# Find exact physical offsets
perl -0777 -ne 'while (/\xc3.\xc8\xc3.\xc8/gs) { printf "Offset: 0x%x\n", (pos() - 6) }' images/disk_org/x1/XDOS_SYS.D88
# Inspect the discovered window
hexdump -C -s 0x00007c13 -n 128 images/disk_org/x1/XDOS_SYS.D88
```

## Evidence
The jump table at `XDOS_SYS.D88` offset `0x7c13` contains a continuous sequence of `C3 xx yy` instructions that perfectly align with the syscall entry points defined in `x-dos.h`:
- Entry 0 (`sys_wopen` at `0xED78`): `C3 76 C8` -> jumps to `0xC876`
- Entry 1 (`sys_wrd` at `0xED7B`): `C3 60 C8` -> jumps to `0xC860`
- Entry 3 (`sys_rdd` at `0xED81`): `C3 6C C8` -> jumps to `0xC86C`
- Entry 4 (`sys_file` at `0xED84`): `C3 98 C8` -> jumps to `0xC898`
- Entry 7 (`sys_devi` at `0xED8D`): `C3 C4 C8` -> jumps to `0xC8C4`
- Entry 8 (`sys_devo` at `0xED90`): `C3 D2 C8` -> jumps to `0xC8D2`
- Entry 10 (`sys_ropen` at `0xED96`): `C3 14 C9` -> jumps to `0xC914`
- Entry 24 (`sys_load` at `0xEDC0`): `C3 AA DE` -> jumps to `0xDEAA`
- Entry 40 (`sys_call` at `0xEDF0`): `C3 1E CA` -> jumps to `0xCA1E`

## Risks
- **Extraction Inconsistency**: The syscall table was NOT found in `XDOSUTIL.D88` at the same offset. This indicates that analysis performed on one disk may not translate directly to another if the kernel is mapped differently or if the disk is not a standard system boot disk.
- **Mapping Contradiction**: There is a remaining discrepancy between the logical record mapping (Record 10 = `fat_area` at `0xEE00`) and the observation that Record 31 (approx) contains the syscall table at `0xED78`. The kernel might be loaded from higher records than previously hypothesized.

## Requested Review
Review the updated `read_path.asm` and `labels.tsv` to ensure the new implementation-level labels (`sys_wopen_impl` etc.) match the intended naming convention for reverse-engineered code.

## Contradictions
The `fat_area` at `0xEE00` (from `make_BGM`) does not align with `Record 10 = Track 1, R=1` if we assume the kernel code is contiguous and Record 10 is near the start. The observed jump table suggests the kernel is loaded from a much later part of the disk (Track 6).

## Provisional Conclusions
The X-DOS kernel syscall table is directly observable in `XDOS_SYS.D88`. The syscalls jump into an implementation region starting around `0xC860`, which validates the hypothesized `0xC800` kernel base address as a likely entry or data region.

## Unknown
The purpose of Entry 2, 5, 6, and 9 (now with confirmed jump targets) remains unknown.

## Notes
- Unrelated local changes were not reset or cleaned.
- Syscall-region bytes were directly observed and extracted from `XDOS_SYS.D88`.
