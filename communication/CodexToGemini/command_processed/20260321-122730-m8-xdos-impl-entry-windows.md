# Gemini Implementation Instruction

## Task ID
20260321-122730-m8-xdos-impl-entry-windows

## Objective
Extract and document direct byte windows for the first few instructions at the implementation entrypoints reached by the confirmed X-DOS syscall jump table, limited to filesystem-relevant read-path syscalls.

## Branch
- Base: `develop`
- Name: `codex/m8-xdos-impl-entry-windows`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-090239-m7-xdos-direct-byte-extraction-report.md`

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
- Scope is limited to entrypoint windows for:
  - `sys_wopen_impl` (`0xC876`)
  - `sys_rdd_impl` (`0xC86C`)
  - `sys_file_impl` (`0xC898`)
  - `sys_devi_impl` (`0xC8C4`)
  - `sys_ropen_impl` (`0xC914`)
- Do not widen scope into write-path modeling beyond these entry windows

## Steps
1. Create branch `codex/m8-xdos-impl-entry-windows` from `develop`.
2. Use the confirmed jump table mapping from M7 to locate the file offsets corresponding to the implementation entrypoints above.
3. Extract a small direct byte window for each target entrypoint from `XDOS_SYS.D88`.
4. Update `analysis/xdos-kernel/read_path.asm` with only directly observed bytes for these entrypoints.
5. Update `analysis/xdos-kernel/labels.tsv` only if a label needs a source/note refinement due to direct extraction.
6. Update `analysis/xdos-kernel/boot_and_io_notes.md` only if needed to record the physical location or extraction-limit note for these implementation entry windows.
7. If any target cannot be mapped confidently, leave it unchanged and record the reason under `unknown` rather than guessing.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m8-xdos-impl-entry-windows`
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
- explicit note stating which entrypoint windows were directly observed and which were not

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- Do not start the next milestone from within this task
