# Gemini Task Instruction

## Task ID
20260323-225156-m24b-xdos-d1b5-extended-control-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m24b-xdos-d1b5-extended-control-catalog`

## Objective
Catalog only directly observed control-transfer instructions inside the newly extended `0xD1B5` window.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

Do not edit any other file.

## Constraints
- Append only.
- Preserve all existing headings, bullets, tables, and comments.
- No semantics.
- No helper scripts.
- No new labels.
- Only direct `call`, `jp`, `jr`, or `ret` observations are allowed.

## Required Work
1. Append a new section to `boot_and_io_notes.md`:
   - `## D1B5 Extended Control Transfers (Analysis-Only)`
2. Add a 4-column table with exactly these columns:
   - `target`
   - `observed transfer`
   - `evidence class`
   - `neutral note`
3. Add rows only for directly visible control-transfer observations within the current extended `0xD1B5` window.
4. In `read_path.asm`, if needed, append only raw transfer comments to the existing `0xD1B5` line, preserving all existing comment content.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

