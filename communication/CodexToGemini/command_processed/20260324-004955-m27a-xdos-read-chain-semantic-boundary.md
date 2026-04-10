# Gemini Task Instruction

## Task ID
20260324-004955-m27a-xdos-read-chain-semantic-boundary

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m27a-xdos-read-chain-semantic-boundary`

## Objective
Reassess the current read-side chain only:
- `0xD1B5`
- `0xD3F7`
- `0xD470`
- `0xD8DA`
- `0xDAB2`

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
   - `## Read-Side Chain Semantic Boundary (Analysis-Only)`
2. Under that heading, add a 3-column table with exactly these columns:
   - `semantic concern`
   - `current evidence grade`
   - `current boundary`
3. Add exactly these rows:
   - `read-side downstream traversal role of 0xD1B5`
   - `read-side downstream traversal role of 0xD3F7`
   - `read-side downstream traversal role of 0xD470`
   - `read-side downstream traversal role of 0xD8DA`
   - `read-side downstream traversal role of 0xDAB2`
4. For `current evidence grade`, use only:
   - `confirmed`
   - `provisional`
   - `unknown`
5. For `current boundary`, write one short conservative sentence per row.
6. In `README.md`, append one short preserving sentence to the existing status/analysis area stating that read-side chain semantic boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
