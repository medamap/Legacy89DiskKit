# Gemini Task Instruction

## Task ID
20260324-010210-m27b-xdos-write-chain-semantic-boundary-retry

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m27b-xdos-write-chain-semantic-boundary-retry`

## Objective
Retry the write-side semantic boundary summary conservatively. Use only already-cataloged evidence and do not assign `provisional` unless the currently documented raw evidence directly justifies it.

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
- Be stricter than the previous attempt.
- If a row cannot be safely upgraded, mark it `unknown`.
- Do not mention nibble-swapping, packed FAM updates, or similar stronger claims unless already directly confirmed in the currently cataloged evidence.

## Required Work
1. Recreate `## Write-Side Chain Semantic Boundary (Analysis-Only)` conservatively.
2. Use the same 3-column table:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Keep exactly these rows:
   - `write-side role of sys_wopen_impl`
   - `write-side role of sys_wrd_impl`
   - `write-side role of helper_c934`
   - `write-side role of helper_c938`
   - `write-side role of helper_c97e`
4. For `current evidence grade`, use only:
   - `confirmed`
   - `provisional`
   - `unknown`
5. If there is any doubt, choose `unknown`.
6. In `README.md`, keep the existing preserving sentence structure, but if needed, revise it so it does not imply more than the table supports.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
