# Gemini Implementation Instruction

## Task ID
20260321-143641-m13b1-xdos-helper-input-pair

## Objective
Prove only that directory bytes `0x1D/0x1E` are consumed as a pair by `helper_d6af`, and document exactly that observation without assigning further meaning.

## Branch
- Base: `develop`
- Name: `codex/m13b1-xdos-helper-input-pair`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-142332-m13a-xdos-directory-entry-boundary-retry-report.md`

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
- Do not inspect content-start correlation in disk images in this task
- Do not assign semantics such as track, sector, FAM, cluster, drive, side, physical, or logical
- Scope is limited to:
  - confirmed entry boundary from M13a
  - `helper_d6af`
  - the fact that bytes `0x1D/0x1E` are read as a pair

## Steps
1. Create branch `codex/m13b1-xdos-helper-input-pair` from `develop`.
2. Re-read M13a to ensure offsets are counted from the proven entry base.
3. Re-read `helper_d6af` and capture the exact byte / instruction sequence that accesses `0x1D` and `0x1E`.
4. Update `analysis/xdos-kernel/boot_and_io_notes.md` with a short evidence-graded section that states only:
   - `0x1D/0x1E` are loaded as a pair from the directory entry
   - the pair is passed into the next helper / traversal stage
   - the pair's external meaning remains unknown
5. Update `analysis/xdos-kernel/README.md` only if the unknown wording improves.
6. Do not modify `read_path.asm` or `labels.tsv`.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13b1-xdos-helper-input-pair`
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
- explicit note stating that no field semantics beyond "consumed as a pair" were assigned

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed until the remaining analysis reaches 100%
- Do not start the next milestone from within this task
