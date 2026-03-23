# Gemini Task Instruction

## Task ID
20260324-012215-m27d-xdos-fam-semantic-boundary

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m27d-xdos-fam-semantic-boundary`

## Objective
Reassess the current FAM-side semantic boundary using only already-cataloged evidence. Do not collect new raw windows.

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
   - `## FAM Semantic Boundary (Analysis-Only)`
2. Under that heading, add a 3-column table with exactly these columns:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Add exactly these rows:
   - `bit-level meaning of sampled FAM values`
   - `meaning of stable high-nibble / low-range observations`
   - `correlation between directory-linked pair and raw FAM byte position`
   - `role of raw FAM windows in shared placement cases`
4. For `current evidence grade`, use only:
   - `confirmed`
   - `provisional`
   - `unknown`
5. For `current boundary`, write one short conservative sentence per row.
6. In `README.md`, append one short preserving sentence to the existing status/analysis area stating that FAM semantic boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
