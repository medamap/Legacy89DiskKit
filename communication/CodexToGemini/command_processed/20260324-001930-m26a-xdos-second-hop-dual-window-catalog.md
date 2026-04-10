# Gemini Task Instruction

## Task ID
20260324-001930-m26a-xdos-second-hop-dual-window-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m26a-xdos-second-hop-dual-window-catalog`

## Objective
Catalog the raw local byte windows for two directly visible second-hop call targets:
- `0xD8DA` from the `0xD3F7` window
- `0xDAB2` from the `0xD470` window

Use one task to capture both raw windows.

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
   - `## Second-Hop Target Byte Windows (Analysis-Only)`
2. Under that heading, add a 4-column table with exactly these columns:
   - `target`
   - `observed bytes`
   - `evidence class`
   - `neutral note`
3. Add exactly two rows:
   - one for `0xD8DA`
   - one for `0xDAB2`
4. In `read_path.asm`, add:
   - `org 0xD8DA` with only directly observed raw bytes from the local window
   - `org 0xDAB2` with only directly observed raw bytes from the local window
5. Do not assign any meaning to either window.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
