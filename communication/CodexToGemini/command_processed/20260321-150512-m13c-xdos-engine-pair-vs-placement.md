# Gemini Implementation Instruction

## Task ID
20260321-150512-m13c-xdos-engine-pair-vs-placement

## Objective
Determine the strongest defensible relationship, if any, between the `0x1D/0x1E` pair and the observed file-placement pair on disk images.

## Branch
- Base: `develop`
- Name: `codex/m13c-xdos-engine-pair-vs-placement`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-142332-m13a-xdos-directory-entry-boundary-retry-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-143641-m13b1-xdos-helper-input-pair-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-145716-m13b2a-xdos-cross-disk-stability.md`

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
- Allowed wording:
  - "observed placement pair"
  - "`0x1D/0x1E` pair"
  - "exact match"
  - "mismatch"
  - "partial correlation"
  - "unknown"
- Do not mention FAM, cluster, physical, logical, drive, side, load address, or entry point
- Do not modify `read_path.asm` or `labels.tsv`

## Steps
1. Create branch `codex/m13c-xdos-engine-pair-vs-placement` from `develop`.
2. Use the confirmed entry-base convention from M13a.
3. Build a compact evidence table for representative files across both disks showing:
   - filename
   - `0x1D/0x1E`
   - observed placement pair on the image
   - whether they exactly match, mismatch, or partially correlate
4. State only the strongest defensible conclusions:
   - whether `0x1D/0x1E` equals the observed placement pair for all files, some files, or no files
   - whether there are contradictions across system files and utility files
   - what remains unknown
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded section for this comparison.
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13c-xdos-engine-pair-vs-placement`
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
- explicit note listing which prohibited semantic labels were intentionally avoided

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed until the remaining analysis reaches 100%
- Do not start the next milestone from within this task
