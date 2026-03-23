# Gemini Task Instruction

## Task ID
20260323-204934-m22b-xdos-downstream-literal-catalog

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m22b-xdos-downstream-literal-catalog`

## Objective
Catalog only the directly observed literal or immediate values that appear inside the already-cataloged downstream target windows reached from `helper_d6af`.

This task is intentionally narrow. Do not assign semantics. Do not describe roles. Do not infer read traversal behavior. Only record observed immediate values and absolute addresses that are directly visible in the local byte windows already documented for:

- `0xD155`
- `0xD753`
- `0xDEE8`
- `0xE00E`

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
- Do not rewrite existing sections.
- Append only.
- Do not change existing headings.
- Do not remove existing bullets or tables.
- Do not add semantics such as `fat lookup`, `allocation`, `traversal`, `directory resolution`, `shared placement resolution`, or `system load`.
- Do not use words such as `implies`, `likely`, `must`, `required`, `role`, `meaning`, `semantic`.

## Required Work
1. In `boot_and_io_notes.md`, append a new section:
   - `## Downstream Target Literal Catalog (Analysis-Only)`
2. Add a 4-column table with exactly these columns:
   - `target`
   - `observed literal/immediate`
   - `evidence class`
   - `neutral note`
3. Only include literals or immediate/address-like values that are directly visible in the already-cataloged windows for the four targets above.
4. Use `confirmed` only when the bytes are directly present in the cataloged window.
5. Neutral notes must stay raw, for example:
   - `immediate value observed in local window`
   - `absolute address literal observed in local window`
6. In `read_path.asm`, if needed, add only short raw comments adjacent to the existing target windows to reflect the same observed literals. Do not add new labels beyond what already exists.

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
- Only a small appended literal catalog is added.
- No new semantics are introduced.
- Existing analysis content remains intact.
