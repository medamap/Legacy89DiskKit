# Gemini Task Report

## Task ID
20260323-212430-m22f-xdos-d1b5-target-window-catalog

## Instruction Filename
20260323-212430-m22f-xdos-d1b5-target-window-catalog.md

## Branch Name
codex/m22f-xdos-d1b5-target-window-catalog

## Summary
Cataloged the target byte window for `0xD1B5` (called from `0xD753`). The raw bytes were extracted from `images/disk_org/x1/XDOS_SYS.D88` at physical offset `0x6050`.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`: Appended a new section with the `0xD1B5` raw byte table.
- `analysis/xdos-kernel/read_path.asm`: Added a new `org 0xD1B5` block with the observed raw bytes.

## Commands
```bash
git checkout -b codex/m22f-xdos-d1b5-target-window-catalog
od -t x1 -j 24656 -N 16 images/disk_org/x1/XDOS_SYS.D88
git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm
```

## Evidence
`0xD753` (offset `0x65EE`) contains `CD B5 D1` (call `0xD1B5`).
Physical offset for `0xD1B5`: `0x65EE - 0x059E = 0x6050`.
Raw bytes at `0x6050` from `XDOS_SYS.D88`: `01 28 03 01 22 05 C5 21`.

## Risks
None. This was an analysis-only, append-only operation.

## Requested Review
Verify the raw byte extraction and the mapping calculation used to locate `0xD1B5`.
