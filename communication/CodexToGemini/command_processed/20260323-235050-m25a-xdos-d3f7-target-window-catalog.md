# Gemini Task Instruction

## Task ID
20260323-235050-m25a-xdos-d3f7-target-window-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m25a-xdos-d3f7-target-window-catalog`

## Objective
Catalog the raw local byte window for target `0xD3F7`, which is directly visible as a local call target inside the second-extended `0xD1B5` window.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

Do not edit any other file.

## Constraints
- Append only.
- No semantics.
- No helper scripts.
- No new markdown files.
- Preserve all existing headings, bullets, tables, and comments.
- Do not discuss roles or meanings.

## Required Work
1. Append a new section to `boot_and_io_notes.md`:
   - `## D3F7 Target Byte Window (Analysis-Only)`
2. Add a 4-column table with exactly these columns:
   - `target`
   - `observed bytes`
   - `evidence class`
   - `neutral note`
3. Add exactly one row for `0xD3F7`.
4. In `read_path.asm`, add a new `org 0xD3F7` block with only the directly observed raw bytes from the local window.
5. Do not assign any meaning to the window.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

