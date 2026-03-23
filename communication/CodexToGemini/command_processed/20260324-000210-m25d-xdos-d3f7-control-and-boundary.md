# Gemini Task Instruction

## Task ID
20260324-000210-m25d-xdos-d3f7-control-and-boundary

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m25d-xdos-d3f7-control-and-boundary`

## Objective
In one slice, catalog directly observed local control transfers inside the already-cataloged `0xD3F7` target window, then add a conservative slice-boundary closeout for `0xD3F7`.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`
- `analysis/xdos-kernel/README.md`

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
   - `## D3F7 Target Control Transfers (Analysis-Only)`
2. Under that heading, add a 4-column table with exactly these columns:
   - `target`
   - `observed transfer`
   - `evidence class`
   - `neutral note`
3. Add rows only for directly visible `call` / `jp` / `jr` / `ret` style control transfers inside the existing `0xD3F7` window.
4. In `read_path.asm`, if needed, append only raw `transfer:` comments to the existing `org 0xD3F7` line, preserving all existing comment content.
5. Then append a second section to `boot_and_io_notes.md`:
   - `## D3F7 Slice Boundary (Analysis-Only)`
6. Under that heading, add a short flat bullet list stating only:
   - `0xD3F7` target byte window cataloged
   - `0xD3F7` literals cataloged
   - `0xD3F7` control transfers cataloged
   - semantic interpretation remains unknown
7. In `README.md`, append one short preserving sentence to the existing `read_path.asm` description mentioning that the `0xD3F7` slice boundary is now annotated.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
