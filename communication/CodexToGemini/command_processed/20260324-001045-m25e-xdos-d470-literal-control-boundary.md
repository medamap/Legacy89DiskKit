# Gemini Task Instruction

## Task ID
20260324-001045-m25e-xdos-d470-literal-control-boundary

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m25e-xdos-d470-literal-control-boundary`

## Objective
In one slice, catalog directly observed literals and directly observed local control transfers inside the already-cataloged `0xD470` target window, then add a conservative slice-boundary closeout for `0xD470`.

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
   - `## D470 Target Literal Catalog (Analysis-Only)`
2. Under that heading, add a 4-column table with exactly these columns:
   - `target`
   - `observed literal/immediate`
   - `evidence class`
   - `neutral note`
3. Add rows only for directly visible immediate/literal values inside the existing `0xD470` window.
4. Then append:
   - `## D470 Target Control Transfers (Analysis-Only)`
5. Under that heading, add a 4-column table with exactly these columns:
   - `target`
   - `observed transfer`
   - `evidence class`
   - `neutral note`
6. Add rows only for directly visible `call` / `jp` / `jr` / `ret` style control transfers inside the existing `0xD470` window.
7. In `read_path.asm`, if needed, append only raw `literal:` and `transfer:` comments to the existing `org 0xD470` line, preserving all existing comment content.
8. Then append a third section to `boot_and_io_notes.md`:
   - `## D470 Slice Boundary (Analysis-Only)`
9. Under that heading, add a short flat bullet list stating only:
   - `0xD470` target byte window cataloged
   - `0xD470` literals cataloged
   - `0xD470` control transfers cataloged
   - semantic interpretation remains unknown
10. In `README.md`, append one short preserving sentence to the existing `read_path.asm` description mentioning that the `0xD470` slice boundary is now annotated.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
