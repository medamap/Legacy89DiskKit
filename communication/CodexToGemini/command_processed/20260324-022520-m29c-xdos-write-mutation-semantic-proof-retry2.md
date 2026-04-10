# Gemini Task Instruction

## Task ID
20260324-022520-m29c-xdos-write-mutation-semantic-proof-retry2

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m29c-xdos-write-mutation-semantic-proof-retry2`

## Objective
Retry the write-side exact mutation semantic proof using the expanded currently cataloged chain:
- `sys_wopen_impl`
- `sys_wrd_impl`
- `helper_c934`
- `helper_c938`
- `helper_c97e`
- `0xC9EA`
- `0xCABA`

Do not collect new raw windows. Test whether the expanded evidence now justifies any narrow `provisional` upgrade.

If it still does not, keep everything `unknown`.

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
- If there is any doubt, choose `unknown`.
- Any `provisional` statement must be narrow and explicitly scoped to the currently cataloged write-side windows only.

## Required Work
1. In `boot_and_io_notes.md`, append:
   - `## Write Mutation Semantic Proof Attempt Retry (Analysis-Only)`
2. Under that heading, add a 3-column table with exactly these columns:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Add exactly these rows:
   - `write-side downstream mutation role of helper_c934 and helper_c938 in sampled windows`
   - `write-side downstream mutation role of 0xC9EA and 0xCABA in sampled windows`
4. For `current evidence grade`, use only:
   - `confirmed`
   - `provisional`
   - `unknown`
5. For `current boundary`, write one short conservative sentence per row.
6. In `README.md`, append one short preserving sentence to the existing status/analysis area stating that a write-mutation semantic proof retry note now exists.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
