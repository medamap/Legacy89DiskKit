# Gemini Task Instruction

## Task ID
20260323-210653-m22c-xdos-downstream-control-transfer-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m22c-xdos-downstream-control-transfer-catalog`

## Objective
Catalog only the directly observed control-transfer instructions that appear inside the already-cataloged downstream target windows for:

- `0xD155`
- `0xD753`
- `0xDEE8`
- `0xE00E`

This task is intentionally narrow. Do not assign semantics. Do not describe roles. Do not infer traversal behavior. Record only direct `call`, `jp`, `jr`, or `ret` observations that are visible in the local target windows already present in the analysis assets.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

Do not edit any other file.

## Required Inputs
- `images/disk_org/x1/XDOS_SYS.D88`
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/read_path.asm`

## Constraints
- No code changes outside the two analysis files above.
- No helper scripts.
- No new markdown files.
- Append only.
- Do not change existing headings.
- Do not remove or rewrite existing sections, bullets, or tables.
- Do not use words such as `meaning`, `semantic`, `role`, `implies`, `likely`, `must`, `required`, `traversal`, `allocation`, `resolution`.

## Required Work
1. In `boot_and_io_notes.md`, append a new section:
   - `## Downstream Target Control Transfers (Analysis-Only)`
2. Add a 4-column table with exactly these columns:
   - `target`
   - `observed transfer`
   - `evidence class`
   - `neutral note`
3. Include only directly visible control-transfer observations from the local byte windows already cataloged for the four targets.
4. Use `confirmed` only when the transfer opcode is directly present in the local window.
5. Neutral notes must stay raw, for example:
   - `local call observed in target window`
   - `local jump observed in target window`
   - `return observed in target window`
6. In `read_path.asm`, if needed, add only short raw comments adjacent to the same target windows. Do not add new labels beyond what already exists.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Completion
- Move this instruction from `command_waiting` to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

## Report Requirements
- `task id`
- `branch_name`
- `summary`
- `changed_files`
- `commands`
- `evidence`
- `risks`
- `requested_review`

## Success Condition
- Only a small appended transfer catalog is added.
- No new semantics are introduced.
- Existing analysis content remains intact.
