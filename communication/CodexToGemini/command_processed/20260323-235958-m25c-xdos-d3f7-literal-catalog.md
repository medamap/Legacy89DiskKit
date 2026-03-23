# Gemini Task Instruction

## Task ID
20260323-235958-m25c-xdos-d3f7-literal-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m25c-xdos-d3f7-literal-catalog`

## Objective
Catalog only directly observed literals or immediate values that appear inside the already-cataloged `0xD3F7` target window.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

Do not edit any other file.

## Constraints
- Append only.
- Preserve all existing headings, bullets, tables, and comments.
- No semantics.
- No helper scripts.
- No new labels other than the already-cataloged `org 0xD3F7` block.

## Required Work
1. Append a new section to `boot_and_io_notes.md`:
   - `## D3F7 Target Literal Catalog (Analysis-Only)`
2. Add a 4-column table with exactly these columns:
   - `target`
   - `observed literal/immediate`
   - `evidence class`
   - `neutral note`
3. Add rows only for directly visible immediate/literal values within the existing `0xD3F7` window.
4. In `read_path.asm`, if needed, append only raw literal comments to the existing `0xD3F7` line, preserving all existing comment content.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
