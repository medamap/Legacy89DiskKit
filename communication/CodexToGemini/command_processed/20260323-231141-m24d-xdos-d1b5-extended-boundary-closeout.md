# Gemini Task Instruction

## Task ID
20260323-231141-m24d-xdos-d1b5-extended-boundary-closeout

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m24d-xdos-d1b5-extended-boundary-closeout`

## Objective
Add a conservative closeout boundary for the current extended `0xD1B5` slice.

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
   - `## D1B5 Extended Slice Boundary (Analysis-Only)`
2. Add only short bullet points stating:
   - the extended `0xD1B5` byte window is cataloged
   - the extended `0xD1B5` literals are cataloged
   - the extended `0xD1B5` control-transfer observation is cataloged
   - semantic interpretation of the extended `0xD1B5` slice remains unknown
3. In `README.md`, append one short sentence to the `read_path.asm` file-structure bullet indicating that extended `0xD1B5` slice boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

