# Gemini Task Instruction

## Task ID
20260323-211047-m22c-xdos-downstream-control-transfer-catalog-retry

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m22c-xdos-downstream-control-transfer-catalog-retry`

## Objective
Retry the downstream control-transfer catalog, but preserve all existing literal observations already present in `read_path.asm`.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

Do not edit any other file.

## Constraints
- Append only.
- Do not remove or replace any existing literal comments from the four downstream target windows.
- If you add control-transfer comments in `read_path.asm`, they must preserve the existing literal observations in the same line.
- No semantics.
- No new labels.
- No helper scripts.

## Required Work
1. Add `## Downstream Target Control Transfers (Analysis-Only)` to `boot_and_io_notes.md` if it is not already present on your branch; otherwise fix it conservatively.
2. Keep the 4-column schema:
   - `target`
   - `observed transfer`
   - `evidence class`
   - `neutral note`
3. Only record directly visible `call`, `jp`, `jr`, or `ret`.
4. In `read_path.asm`, preserve the existing literal comments and append transfer notes without deleting prior observations.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

