# Gemini Task Instruction

## Task ID
20260324-013610-m28a-xdos-shared-placement-semantic-proof-retry

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m28a-xdos-shared-placement-semantic-proof-retry`

## Objective
Retry the shared-placement semantic proof attempt conservatively. Use only already-cataloged evidence and do not upgrade to `provisional` unless the currently documented raw evidence directly justifies it.

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
- If there is any doubt, choose `unknown`.
- Do not claim `1D` equals a logical track identifier.
- Do not claim `1E` equals a unique physical sector identifier.
- Do not claim absence of physical overlap unless directly proven by current cataloged evidence.

## Required Work
1. Recreate `## Shared Placement Semantic Proof Attempt (Analysis-Only)` conservatively.
2. Use the same 3-column table:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Keep exactly these rows:
   - `shared track-level region interpretation for sampled 2D cases`
   - `meaning of same-1D-different-1E pattern in sampled 2D cases`
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
