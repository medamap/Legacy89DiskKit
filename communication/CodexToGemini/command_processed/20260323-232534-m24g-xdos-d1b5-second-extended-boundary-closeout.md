# Gemini Task Instruction

## Task ID
20260323-232534-m24g-xdos-d1b5-second-extended-boundary-closeout

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m24g-xdos-d1b5-second-extended-boundary-closeout`

## Objective
Add a conservative closeout boundary for the current second-extended `0xD1B5` slice.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

Do not edit any other file.

## Constraints
- No new observations.
- No semantics.
- Append only.
- Preserve all existing headings, bullets, tables, and wording.

## Required Work
1. In `boot_and_io_notes.md`, append a new section:
   - `## D1B5 Second Extended Slice Boundary (Analysis-Only)`
2. Add only short bullet points stating:
   - the second-extended `0xD1B5` byte window is cataloged
   - the second-extended `0xD1B5` literals are cataloged
   - the second-extended `0xD1B5` control-transfer observations are cataloged
   - semantic interpretation of the second-extended `0xD1B5` slice remains unknown
3. In `README.md`, append one short sentence to the `read_path.asm` file-structure bullet indicating that second-extended `0xD1B5` slice boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

