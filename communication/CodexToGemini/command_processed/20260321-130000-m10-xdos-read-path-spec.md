# Gemini Implementation Instruction

## Task ID
20260321-130000-m10-xdos-read-path-spec

## Objective
Consolidate the directly observed X-DOS read-path evidence into a conservative pseudo-spec that explains the observed role split between `sys_file`, `sys_ropen`, `sys_rdd`, helper routines, and downstream delegates.

## Branch
- Base: `develop`
- Name: `codex/m10-xdos-read-path-spec`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-090239-m7-xdos-direct-byte-extraction-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-122730-m8-xdos-impl-entry-windows-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-125445-m9-xdos-read-helper-windows-report.md`

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
- Do not invent bytes, synthesized instructions, or guessed targets
- Keep the pseudo-spec conservative and evidence-graded
- Distinguish explicitly between:
  - direct-byte observations
  - immediate instruction-level inferences
  - broader behavioral hypotheses
- Scope is limited to the read path and its directly observed helpers/delegates
- Do not start write-path or boot-path reconstruction in this task

## Steps
1. Create branch `codex/m10-xdos-read-path-spec` from `develop`.
2. Re-read the accepted M7-M9 reports and current analysis assets.
3. Produce a conservative read-path spec inside the analysis workspace by updating existing analysis files only.
4. The spec must cover:
   - what `sys_file` directly does at entry
   - what `sys_ropen` directly does at entry
   - what `sys_rdd` directly does at entry
   - which helpers each one calls or jumps to
   - where direct observation ends and hypothesis begins
5. Prefer updating `analysis/xdos-kernel/boot_and_io_notes.md` with a dedicated "Read Path Spec" section.
6. Update `analysis/xdos-kernel/read_path.asm` and `analysis/xdos-kernel/labels.tsv` only if a small clarification is needed to support the spec.
7. Do not over-normalize or rewrite existing files beyond what the spec needs.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m10-xdos-read-path-spec`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
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
- explicit note stating what parts of the read-path spec are direct observation vs inference

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- Do not start the next milestone from within this task
