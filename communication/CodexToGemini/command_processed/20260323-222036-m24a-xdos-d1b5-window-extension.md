# Gemini Task Instruction

## Task ID
20260323-222036-m24a-xdos-d1b5-window-extension

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m24a-xdos-d1b5-window-extension`

## Objective
Extend the raw byte window for target `0xD1B5` beyond the initial 8-byte slice, without assigning semantics.

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

## Required Work
1. Append a new section to `boot_and_io_notes.md`:
   - `## D1B5 Extended Byte Window (Analysis-Only)`
2. Add a 4-column table with exactly these columns:
   - `target`
   - `observed bytes`
   - `evidence class`
   - `neutral note`
3. Add exactly one row for an extended `0xD1B5` local window that goes beyond the current 8-byte span.
4. In `read_path.asm`, extend the existing `org 0xD1B5` raw byte line to include the newly observed bytes, preserving all existing comment content.
5. Do not decode meaning. Do not infer roles.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

