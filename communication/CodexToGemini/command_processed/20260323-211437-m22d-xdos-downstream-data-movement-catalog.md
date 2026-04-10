# Gemini Task Instruction

## Task ID
20260323-211437-m22d-xdos-downstream-data-movement-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m22d-xdos-downstream-data-movement-catalog`

## Objective
Catalog only directly observed data-movement or address-load style instructions that appear inside the already-cataloged downstream target windows for:

- `0xD155`
- `0xD753`
- `0xDEE8`
- `0xE00E`

This task is still analysis-only. Do not assign semantics. Do not describe roles. Record only local raw observations such as `ld rr,nn`, `ld r,n`, `inc`, `dec`, `ex`, or similar directly visible instructions in the local windows.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

Do not edit any other file.

## Constraints
- Append only.
- Preserve existing literal and control-transfer comments in `read_path.asm`.
- If you add more inline comments, append without deleting prior observations.
- No helper scripts.
- No new labels.
- No words such as `meaning`, `semantic`, `role`, `implies`, `likely`, `must`, `required`, `traversal`, `allocation`, `resolution`.

## Required Work
1. Append a new section to `boot_and_io_notes.md`:
   - `## Downstream Target Data-Movement Windows (Analysis-Only)`
2. Add a 4-column table:
   - `target`
   - `observed instruction pattern`
   - `evidence class`
   - `neutral note`
3. Include only directly visible local instruction patterns from the four target windows.
4. Use `confirmed` only for directly visible patterns.
5. In `read_path.asm`, if needed, append raw comments while preserving all existing comment content on the same lines.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

