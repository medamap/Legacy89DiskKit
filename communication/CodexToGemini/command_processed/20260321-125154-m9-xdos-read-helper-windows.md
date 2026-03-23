# Gemini Implementation Instruction

## Task ID
20260321-125154-m9-xdos-read-helper-windows

## Objective
Extend the X-DOS read-path analysis by extracting direct byte windows for helper routines reached from the confirmed syscall implementation entrypoints, and document the role split between `sys_file`, `sys_ropen`, `sys_rdd`, and those helpers.

## Branch
- Base: `develop`
- Name: `codex/m9-xdos-read-helper-windows`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-122730-m8-xdos-impl-entry-windows-report.md`

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
- Use direct byte extraction from `XDOS_SYS.D88`
- Do not invent filler bytes, synthesized instructions, or guessed jump targets
- Scope is limited to read-path helper routines directly reached from already confirmed entrypoints
- Target helper addresses:
  - `0xC934`
  - `0xC97E`
  - `0xC9BC`
  - `0xD6AF`
- If a helper cannot be mapped confidently, leave it unchanged and record the reason under `unknown`

## Steps
1. Create branch `codex/m9-xdos-read-helper-windows` from `develop`.
2. Use the confirmed mapping from M7/M8 to derive file offsets for helper addresses `0xC934`, `0xC97E`, `0xC9BC`, and `0xD6AF`.
3. Extract small direct byte windows for each helper from `XDOS_SYS.D88`.
4. Update `analysis/xdos-kernel/read_path.asm` with only directly observed bytes for these helper routines.
5. Update `analysis/xdos-kernel/labels.tsv` only if a helper label, note, or source needs refinement.
6. Update `analysis/xdos-kernel/boot_and_io_notes.md` with a short read-path helper note summarizing:
   - what `sys_file` appears to do
   - what `sys_ropen` appears to do
   - what `sys_rdd` does at entry versus where it delegates
   - which helpers are now directly observed
7. Keep conclusions conservative. If a routine’s purpose is still unclear, say so.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m9-xdos-read-helper-windows`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/boot_and_io_notes.md`
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
- explicit note stating which helper windows were directly observed and which were not

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- Do not start the next milestone from within this task
