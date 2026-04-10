# Gemini Implementation Instruction

## Task ID
20260321-144228-m13b2-xdos-content-start-pair-correlation

## Objective
Determine the strongest defensible correlation, if any, between directory bytes `0x1B/0x1C` and the observed start location of file content on `XDOS_SYS.D88` and `XDOSUTIL.D88`.

## Branch
- Base: `develop`
- Name: `codex/m13b2-xdos-content-start-pair-correlation`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-142332-m13a-xdos-directory-entry-boundary-retry-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-143641-m13b1-xdos-helper-input-pair-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-140734-m13-xdos-dir-byte-placement-correlation-report-retry2.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- This is an analysis-only task; do not modify C# or C++ product code
- Do not reset, stash, revert, or otherwise clean unrelated local changes
- Ignore unrelated modified or untracked files unless they block your target files
- Use the confirmed entry-base convention from M13a
- Do not analyze `0x1D/0x1E` in this task except to mention they are separate from `0x1B/0x1C`
- Do not call `0x1B/0x1C` "FAM", "cluster", "physical", "logical", "drive", or "side" unless directly proven
- Allowed wording:
  - "observed content-start pair"
  - "track-like / sector-like correlation"
  - "content-start correlation"
  - "unknown"
- Do not modify `read_path.asm` or `labels.tsv`

## Steps
1. Create branch `codex/m13b2-xdos-content-start-pair-correlation` from `develop`.
2. Re-read M13a so the byte indices are counted from the correct entry base.
3. Build a compact evidence table for representative files across both disks showing:
   - filename
   - `0x1B/0x1C`
   - observed content-start bytes/position in the image
4. Derive only the strongest defensible statements:
   - whether `0x1B/0x1C` correlate with an observed content-start pair
   - whether that correlation appears stable across both disks
   - what remains unknown
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded section for `0x1B/0x1C`.
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13b2-xdos-content-start-pair-correlation`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md`
- `git status --short`

## Deliverable
- Markdown report in `communication/GeminiToCodex/report_waiting/`

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
- contradictions
- provisional conclusions
- unknown
- explicit note confirming that unrelated local changes were not reset or cleaned
- explicit note listing which terms were intentionally avoided because they remain unproven

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed until the remaining analysis reaches 100%
- Do not start the next milestone from within this task
