# Gemini Implementation Instruction

## Task ID
20260321-141608-m13a-xdos-directory-entry-boundary

## Objective
Determine the fixed entry boundary and byte indexing convention for X-DOS directory entries on `XDOS_SYS.D88` and `XDOSUTIL.D88` before assigning semantics to any offsets.

## Branch
- Base: `develop`
- Name: `codex/m13a-xdos-directory-entry-boundary`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-135131-m13-xdos-dir-byte-placement-correlation-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-140012-m13-xdos-dir-byte-placement-correlation-report.md`

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
- Do not assign semantics to `0x1A/0x1B` or `0x1D/0x1E` in this task
- Do not modify `read_path.asm` or `labels.tsv`
- Goal is only to establish:
  - where an entry begins
  - entry length
  - where filename bytes sit inside the entry
  - how byte offsets should be counted from the true entry base

## Steps
1. Create branch `codex/m13a-xdos-directory-entry-boundary` from `develop`.
2. Inspect representative contiguous directory bytes in `XDOS_SYS.D88` and `XDOSUTIL.D88`.
3. Identify repeated entry cadence and prove whether entries are fixed-length or variable-length.
4. Build a small table with at least three adjacent entries showing:
   - absolute disk-image offset of entry start
   - filename span inside the entry
   - entry length
   - where byte indices `0x1A`, `0x1B`, `0x1D`, `0x1E` land within the raw bytes
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with a dedicated "Directory Entry Boundary" section.
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown list becomes clearer.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13a-xdos-directory-entry-boundary`
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
- explicit statement of the proven entry base and the proven byte-indexing convention

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed until the remaining analysis reaches 100%
- Do not start the next milestone from within this task
