# Gemini Task Instruction

## Task ID
20260324-011145-m27c-xdos-boot-rule-semantic-boundary

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m27c-xdos-boot-rule-semantic-boundary`

## Objective
Reassess the current boot-rule / early-area semantic boundary using only already-cataloged evidence. Do not collect new raw windows.

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
   - `## Boot Rule Semantic Boundary (Analysis-Only)`
2. Under that heading, add a 3-column table with exactly these columns:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Add exactly these rows:
   - `boot-rule role of Track 0 Head 0 early-area span`
   - `boot-rule role of Track 0 Head 1 split observations`
   - `boot-rule role of Track 1 Head 0 cross-disk equality`
   - `boot-rule role of geometry translation (C * 2 + H, R)`
4. For `current evidence grade`, use only:
   - `confirmed`
   - `provisional`
   - `unknown`
5. For `current boundary`, write one short conservative sentence per row.
6. In `README.md`, append one short preserving sentence to the existing status/analysis area stating that boot-rule semantic boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
