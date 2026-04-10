# Gemini Implementation Instruction

## Task ID
20260321-131146-m11-xdos-write-path-spec

## Objective
Start the X-DOS write-path reconstruction by consolidating directly observed evidence for `sys_wopen`, `sys_wrd`, and their helper routines into a conservative pseudo-spec.

## Branch
- Base: `develop`
- Name: `codex/m11-xdos-write-path-spec`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-090239-m7-xdos-direct-byte-extraction-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-122730-m8-xdos-impl-entry-windows-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-125445-m9-xdos-read-helper-windows-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`

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
- Do not invent bytes, synthesized instructions, or guessed jump targets
- Keep the write-path spec conservative and evidence-graded
- Distinguish explicitly between:
  - direct-byte observations
  - immediate instruction-level inferences
  - broader behavioral hypotheses
- Scope is limited to:
  - `sys_wopen`
  - `sys_wrd`
  - `sys_wopen_impl`
  - `sys_wrd_impl`
  - `helper_c934`
  - `helper_c97e`
- Do not claim full disk-full semantics or cluster-allocation policy unless directly supported
- Do not start boot-path reconstruction in this task

## Steps
1. Create branch `codex/m11-xdos-write-path-spec` from `develop`.
2. Re-read the existing direct-byte findings for the write-related jump-table entries, implementation entrypoints, and helpers.
3. Extend the analysis notes with a dedicated conservative write-path spec.
4. The spec must cover:
   - what `sys_wopen` directly does at entry
   - what `sys_wrd` directly does at entry
   - how `helper_c934` and `helper_c97e` are used from those routines
   - where direct observation ends and write-path hypothesis begins
5. Prefer updating `analysis/xdos-kernel/boot_and_io_notes.md` with a dedicated "Write Path Spec" section.
6. Update `analysis/xdos-kernel/read_path.asm` and `analysis/xdos-kernel/labels.tsv` only if a small clarification is needed to support the spec.
7. Keep conclusions conservative. If a behavior such as "Disk full handling" cannot be justified from the observed bytes, mark it as `unknown`.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m11-xdos-write-path-spec`
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
- explicit note stating what parts of the write-path spec are direct observation vs inference

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- Do not start the next milestone from within this task
