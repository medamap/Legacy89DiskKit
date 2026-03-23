# Gemini Implementation Instruction

## Task ID
20260321-145041-m13b2-xdos-content-start-pair-correlation-retry

## Objective
Retry M13b2 with a narrower goal: prove only whether `0x1B/0x1C` do or do not correlate with the observed content-start location. Do not assign any alternative meaning if correlation is absent.

## Branch
- Base: `develop`
- Name: `codex/m13b2-xdos-content-start-pair-correlation-retry`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-142332-m13a-xdos-directory-entry-boundary-retry-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-143641-m13b1-xdos-helper-input-pair-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-144228-m13b2-xdos-content-start-pair-correlation-report.md`

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
- Do not analyze or mention alternative meanings such as load address, entry point, FAM pointer, cluster, physical, logical, drive, or side
- Do not analyze `0x1D/0x1E` in this task except to say they are a different pair and outside current scope
- Allowed wording:
  - "observed content-start pair"
  - "content-start correlation"
  - "no stable correlation observed"
  - "unknown"
- Do not modify `read_path.asm` or `labels.tsv`

## Steps
1. Create branch `codex/m13b2-xdos-content-start-pair-correlation-retry` from `develop`.
2. Re-read the failed report and remove every claim beyond correlation / non-correlation.
3. Build a compact evidence table for representative files across both disks showing:
   - filename
   - `0x1B/0x1C`
   - observed content-start pair
4. From that table, state only:
   - whether `0x1B/0x1C` correlate with observed content-start
   - whether the correlation is stable across both disks
   - what remains unknown
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded section for `0x1B/0x1C`.
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13b2-xdos-content-start-pair-correlation-retry`
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
- explicit note stating that no alternative semantics were assigned to `0x1B/0x1C`

## Advancement Rule
- This retry is allowed automatically because the previous report was not accepted
- Do not start the next milestone from within this task
