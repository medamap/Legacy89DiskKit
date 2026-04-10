# Gemini Work Report

## Task ID
20260323-211437-m22d-xdos-downstream-data-movement-catalog

## Instruction
20260323-211437-m22d-xdos-downstream-data-movement-catalog.md

## Branch Name
none

## Summary
Cataloged directly observed data-movement and address-load instruction patterns for four downstream targets in the X-DOS kernel. Added a new analysis section to `boot_and_io_notes.md` and appended raw comments to `read_path.asm`.

## Changed Files
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

## Commands
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Evidence
- `analysis/xdos-kernel/boot_and_io_notes.md`: New section `## Downstream Target Data-Movement Windows (Analysis-Only)` added with a 4-column table.
- `analysis/xdos-kernel/read_path.asm`: Lines for targets `0xD155`, `0xD753`, `0xDEE8`, and `0xE00E` updated with instruction pattern comments.

## Risks
- Direct observation only; no semantics assigned to instructions.
- Assumed Z80 instruction set for decoding (e.g., `rst 0x18` for `0xDF`).

## Requested Review
- Please verify the instruction patterns for the four targets match the raw byte sequences in `read_path.asm`.
