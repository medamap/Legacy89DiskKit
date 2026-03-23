# Gemini Implementation Instruction

## Task ID
20260321-145716-m13b2a-xdos-cross-disk-stability

## Objective
Prove only whether directory bytes `0x1B/0x1C` are stable across disks for identical files. Do not compare them to any other field in this task.

## Branch
- Base: `develop`
- Name: `codex/m13b2a-xdos-cross-disk-stability`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-145041-m13b2-xdos-content-start-pair-correlation-retry-report.md`

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
- Do not compare `0x1B/0x1C` to `0x1D/0x1E`
- Do not mention content-start, load address, entry point, FAM, cluster, physical, logical, drive, or side
- Allowed wording:
  - "cross-disk stability"
  - "same pair observed"
  - "different pair observed"
  - "unknown"
- Do not modify `read_path.asm` or `labels.tsv`

## Steps
1. Create branch `codex/m13b2a-xdos-cross-disk-stability` from `develop`.
2. Identify files that appear on both `XDOS_SYS.D88` and `XDOSUTIL.D88`.
3. Build a compact table showing, for each shared file:
   - filename
   - disk
   - `0x1B/0x1C`
4. State only:
   - whether `0x1B/0x1C` is stable across disks for the sampled identical files
   - what remains unknown
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with a short evidence-graded section.
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13b2a-xdos-cross-disk-stability`
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
- explicit note stating that no comparison to other fields was made in this task

## Advancement Rule
- This retry/refinement is allowed automatically because the previous report was not accepted
- Do not start the next milestone from within this task
