# Gemini Task Instruction

## Task ID
20260323-212108-m22e-xdos-downstream-target-boundary

## Task Kind
analysis

## Branch
- Base: `develop`
- New branch: `codex/m22e-xdos-downstream-target-boundary`

## Objective
Close out the current downstream-target analysis slice by adding a conservative boundary summary only.

## Files You May Edit
- `analysis/xdos-kernel/boot_and_io_notes.md`
- `analysis/xdos-kernel/README.md`

Do not edit any other file.

## Constraints
- No new observations.
- No semantics.
- No new helper scripts.
- Preserve existing headings and bullets.
- Append only.
- Use only `confirmed` or `unknown`.

## Required Work
1. In `boot_and_io_notes.md`, append a new section:
   - `## Downstream Target Boundary (Analysis-Only)`
2. Add a short table or bullet summary that states only:
   - downstream target byte windows are cataloged
   - local literals are cataloged
   - local control transfers are cataloged
   - local `0xDEE8` address-load style observation is cataloged
   - downstream semantic interpretation remains unknown
3. In `README.md`, append only one short sentence to the relevant analysis bullet indicating that downstream target boundary notes now exist.

## Validation
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Completion
- Move this instruction to `command_processing` before work starts.
- Move it to `command_processed` after work completes.
- Submit a markdown report to `communication/GeminiToCodex/report_waiting/`.

