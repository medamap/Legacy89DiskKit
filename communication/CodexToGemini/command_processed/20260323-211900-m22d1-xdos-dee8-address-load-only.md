# Gemini Task Instruction

## Task ID
20260323-211900-m22d1-xdos-dee8-address-load-only

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m22d1-xdos-dee8-address-load-only`

## Objective
Add a single raw address-load style observation for target `0xDEE8` only.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

## Constraints
- Edit only the two files above.
- Append only.
- Preserve all existing literal and control-transfer comments.
- Do not touch any other target besides `0xDEE8`.
- Do not mention `jr`, `call`, `ret`, `rst`, or any semantics.

## Required Work
1. Append a new section to `boot_and_io_notes.md`:
   - `## Downstream Address-Load Observation (Analysis-Only)`
2. Add exactly one table row for `0xDEE8` with 4 columns:
   - `target`
   - `observed instruction pattern`
   - `evidence class`
   - `neutral note`
3. Limit the pattern to the directly visible local sequence built from:
   - `ld bc, 0x0140`
   - `ld de, 0x00A8`
   - `ld hl, 0xEE00`
   - `add hl, de`
4. In `read_path.asm`, only append a raw comment for `0xDEE8`, preserving all existing comment content on that line.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

