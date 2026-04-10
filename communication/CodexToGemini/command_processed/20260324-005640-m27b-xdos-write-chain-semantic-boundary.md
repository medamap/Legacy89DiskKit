# Gemini Task Instruction

## Task ID
20260324-005640-m27b-xdos-write-chain-semantic-boundary

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m27b-xdos-write-chain-semantic-boundary`

## Objective
Reassess the current write-side chain only, using already-cataloged evidence and already-documented write-path windows:
- `sys_wopen_impl`
- `sys_wrd_impl`
- `helper_c934`
- `helper_c938`
- `helper_c97e`

Do not collect new raw windows. Use only already-cataloged evidence to determine whether the current semantic claim can be upgraded at all, or whether it must remain blocked.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

Do not edit any other file.

## Constraints
- Append only.
- Preserve all existing headings, bullets, tables, and comments.
- No new raw catalog sections.
- No helper scripts.
- No new markdown files.
- No strong semantics unless directly supported by already-cataloged evidence.
- If an upgrade is not justified, say so explicitly.

## Required Work
1. In `boot_and_io_notes.md`, append:
   - `## Write-Side Chain Semantic Boundary (Analysis-Only)`
2. Under that heading, add a 3-column table with exactly these columns:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Add exactly these rows:
   - `write-side role of sys_wopen_impl`
   - `write-side role of sys_wrd_impl`
   - `write-side role of helper_c934`
   - `write-side role of helper_c938`
   - `write-side role of helper_c97e`
4. For `current evidence grade`, use only:
   - `confirmed`
   - `provisional`
   - `unknown`
5. For `current boundary`, write one short conservative sentence per row.
6. In `README.md`, append one short preserving sentence to the existing status/analysis area stating that write-side chain semantic boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
