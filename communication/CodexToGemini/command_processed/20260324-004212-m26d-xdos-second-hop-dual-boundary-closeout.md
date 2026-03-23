# Gemini Task Instruction

## Task ID
20260324-004212-m26d-xdos-second-hop-dual-boundary-closeout

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m26d-xdos-second-hop-dual-boundary-closeout`

## Objective
Close out the current raw-analysis slices for both already-cataloged second-hop targets:
- `0xD8DA`
- `0xDAB2`

Use one task to add conservative boundary notes for both slices.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

Do not edit any other file.

## Constraints
- Append only.
- Preserve all existing headings, bullets, tables, and comments.
- No semantics.
- No helper scripts.
- No new markdown files.
- Do not discuss roles or meanings.

## Required Work
1. In `boot_and_io_notes.md`, append:
   - `## Second-Hop Slice Boundary (Analysis-Only)`
2. Under that heading, add a short flat bullet list stating only:
   - `0xD8DA` target byte window cataloged
   - `0xD8DA` literals cataloged
   - `0xD8DA` control transfers cataloged
   - `0xDAB2` target byte window cataloged
   - `0xDAB2` literals cataloged
   - `0xDAB2` control transfers cataloged
   - semantic interpretation remains unknown
3. In `README.md`, append one short preserving sentence to the existing `read_path.asm` description mentioning that second-hop slice boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.
