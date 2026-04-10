# Gemini Task Instruction

## Task ID
20260324-003412-m26c-xdos-second-hop-dual-control-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m26c-xdos-second-hop-dual-control-catalog`

## Objective
Catalog directly observed local control transfers inside both already-cataloged second-hop target windows:
- `0xD8DA`
- `0xDAB2`

Use one task to capture both control-transfer catalogs.

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
   - `## Second-Hop Target Control Transfers (Analysis-Only)`
2. Under that heading, add a 4-column table with exactly these columns:
   - `target`
   - `observed transfer`
   - `evidence class`
   - `neutral note`
3. Add rows only for directly visible `call` / `jp` / `jr` / `ret` style control transfers inside the existing `0xD8DA` and `0xDAB2` windows.
4. In `read_path.asm`, if needed, append only raw `transfer:` comments to the existing `org 0xD8DA` and `org 0xDAB2` lines, preserving all existing comment content.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
