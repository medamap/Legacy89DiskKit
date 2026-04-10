# Gemini Implementation Instruction

## Task ID
20260321-004249-m5-xdos-byte-window-reconstruction

## Objective
Retry the X-DOS byte-window reconstruction task. The current result is close, but two issues need correction: one branch-target annotation in the FDC wait loop appears inconsistent with the raw bytes, and evidence classification for FDC-related labels is not consistent across files.

## Branch
- Base: `develop`
- Name: `codex/m5-xdos-byte-window-reconstruction`
- Continue on the same branch if possible
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Files To Read First
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-004249-m5-xdos-byte-window-reconstruction.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-004249-m5-xdos-byte-window-reconstruction.md)

## Retry Reasons
- In `fdc_wait_loop`, the bytes `38 FB` do not obviously target the label `fdc_wait_loop` itself; the comment should be corrected to the actual local target or made more conservative
- FDC-related evidence classes are inconsistent between `labels.tsv` and `boot_and_io_notes.md`

## Constraints
- Keep scope narrow
- Do not add new byte windows unless needed to explain the correction
- Do not invent opcode bytes
- Prefer the most conservative wording if there is any uncertainty

## Required Changes
1. Correct the FDC wait loop annotation in `analysis/xdos-kernel/read_path.asm`.
   - Recalculate the branch target from the observed bytes
   - If the exact target is the `in a,(c)` instruction rather than the window label start, reflect that
   - If the safest wording is a raw branch-offset note instead of a symbolic target, use that

2. Reconcile FDC evidence classes across:
   - `analysis/xdos-kernel/labels.tsv`
   - `analysis/xdos-kernel/boot_and_io_notes.md`
   - `analysis/xdos-kernel/read_path.asm` comments if needed
   - If only the status polling at `0x0FF8` is truly evidenced by bytes, do not over-promote neighboring ports without support

3. Keep the rest of the byte-window work intact unless a small wording correction is needed.

## Verification
- Show the corrected `fdc_wait_loop` snippet
- Show the final FDC-related rows in `labels.tsv`
- Show the final FDC section in `boot_and_io_notes.md`

## Deliverable
Write one Markdown report to:

- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_waiting`

After completion:

- Move this instruction file to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/CodexToGemini/command_processed`

## Report Requirements
- task id
- instruction filename
- branch_name
- summary
- changed_files
- commands
- evidence
- risks
- requested_review

## Expected Result
- The byte-window reconstruction remains conservative
- No misleading branch-target annotation remains
- FDC evidence grading is internally consistent
