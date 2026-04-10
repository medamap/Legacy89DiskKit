# Gemini Implementation Instruction

## Task ID
20260321-142332-m13a-xdos-directory-entry-boundary-retry

## Objective
Retry M13a with zero field semantics. Prove only the raw directory entry boundary, fixed length, and filename byte span.

## Branch
- Base: `develop`
- Name: `codex/m13a-xdos-directory-entry-boundary-retry`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-141608-m13a-xdos-directory-entry-boundary-report.md`
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
- Do not assign any meaning to bytes `0x1A/0x1B/0x1D/0x1E`
- Do not mention track, sector, cluster, FAM, physical, logical, content-start, or helper semantics in conclusions
- Do not modify `read_path.asm` or `labels.tsv`
- Only prove:
  - where a directory entry starts
  - entry length
  - where the filename starts and ends
  - how to count byte indices from the true entry base

## Steps
1. Create branch `codex/m13a-xdos-directory-entry-boundary-retry` from `develop`.
2. Inspect adjacent raw directory bytes in both `XDOS_SYS.D88` and `XDOSUTIL.D88`.
3. Produce a compact evidence table with at least three consecutive entries showing:
   - absolute image offset of entry start
   - absolute image offset of filename start
   - filename bytes
   - entry length
   - where byte indices `0x1A`, `0x1B`, `0x1D`, `0x1E` land within the raw 32-byte block
4. Update `analysis/xdos-kernel/boot_and_io_notes.md` with a boundary-only section.
5. Update `analysis/xdos-kernel/README.md` only if it helps clarify that field semantics are still open.
6. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13a-xdos-directory-entry-boundary-retry`
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
- explicit note stating that no field semantics were assigned in this retry

## Advancement Rule
- This retry is allowed automatically because the previous report was not accepted
- Do not start the next milestone from within this task
