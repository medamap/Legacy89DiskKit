# Gemini Task Instruction

## Task ID
20260324-021045-m29a-xdos-caba-target-window-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m29a-xdos-caba-target-window-catalog`

## Objective
Catalog the raw local byte window for target `0xCABA`, which is directly visible as a local jump target inside the already-cataloged `0xC9EA` window.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

Do not edit any other file.

## Constraints
- Append only.
- Preserve all existing headings, bullets, tables, and comments.
- No semantics.
- No helper scripts.
- No new markdown files.
- Do not discuss roles or meanings.

## Required Work
1. In `boot_and_io_notes.md`, append:
   - `## CABA Target Byte Window (Analysis-Only)`
2. Under that heading, add a 4-column table with exactly these columns:
   - `target`
   - `observed bytes`
   - `evidence class`
   - `neutral note`
3. Add exactly one row for `0xCABA`.
4. In `read_path.asm`, add a new `org 0xCABA` block with only the directly observed raw bytes from the local window.
5. Do not assign any meaning to the window.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
