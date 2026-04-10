# Gemini Task Instruction

## Task ID
20260324-013020-m28a-xdos-shared-placement-semantic-proof

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m28a-xdos-shared-placement-semantic-proof`

## Objective
Try to move one real semantic theme forward: shared placement.

Use only already-cataloged evidence to test whether the currently observed pattern
- same track-level sharing
- same `0x1D`
- different `0x1E`

is strong enough to justify upgrading shared-placement understanding from pure `unknown` to a narrowly-scoped `provisional` statement.

If the evidence is still insufficient, say so explicitly and keep it `unknown`.

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
- If an upgrade is not justified, keep `unknown`.
- If an upgrade is justified, it must be narrow and explicitly scoped to the sampled 2D cases only.

## Required Work
1. In `boot_and_io_notes.md`, append:
   - `## Shared Placement Semantic Proof Attempt (Analysis-Only)`
2. Under that heading, add a 3-column table with exactly these columns:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Add exactly these rows:
   - `shared track-level region interpretation for sampled 2D cases`
   - `meaning of same-1D-different-1E pattern in sampled 2D cases`
4. For `current evidence grade`, use only:
   - `confirmed`
   - `provisional`
   - `unknown`
5. For `current boundary`, write one short conservative sentence per row.
6. In `README.md`, append one short preserving sentence to the existing status/analysis area stating that a shared-placement semantic proof attempt note now exists.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
