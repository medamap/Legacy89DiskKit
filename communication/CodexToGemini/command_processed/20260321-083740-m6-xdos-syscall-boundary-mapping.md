# Gemini Implementation Instruction

## Task ID
20260321-083740-m6-xdos-syscall-boundary-mapping

## Objective
Grow the X-DOS kernel analysis workspace by conservatively mapping code/data boundaries around filesystem-relevant syscall entrypoints and nearby observed byte windows, without inventing bytes or changing product code.

## Branch
- Base: `develop`
- Name: `codex/m6-xdos-syscall-boundary-mapping`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/x1_io_ports_reference.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-001005-m2-xdos-read-path-analysis-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-005044-m5-xdos-byte-window-reconstruction-report.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- This is an analysis-only task; do not modify C# or C++ product code
- Do not rewrite unrelated communication history
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Do not invent filler bytes, synthesized instructions, or guessed jump targets
- If a region is only known by label and sparse bytes, keep it as labels plus `db` and comments
- Scope is limited to filesystem-relevant syscall entrypoints, FDC wait/dispatch context, side-select logic, and nearby code/data boundaries
- Do not start write-path or boot-path reconstruction in this task

## Steps
1. Create branch `codex/m6-xdos-syscall-boundary-mapping` from `develop`.
2. Review the current analysis workspace and accepted M2/M5 reports to identify the next conservative expansion points.
3. Extend `analysis/xdos-kernel/read_path.asm` with additional observed byte windows or boundary annotations around these labels where supported by primary evidence:
   - `sys_file`
   - `sys_ropen`
   - `sys_rdd`
   - `sys_devi`
   - `side_select_logic`
   - `fdc_wait_loop`
4. Update `analysis/xdos-kernel/labels.tsv` with any newly justified labels or with evidence-class refinements caused by the new byte windows.
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` or `analysis/xdos-kernel/README.md` only if needed to document a newly established code/data-boundary rule or a newly observed filesystem-relevant hardware interaction.
6. Keep every new statement tied to one of:
   - direct bytes observed from X-DOS artifacts
   - salvaged source
   - previously accepted analysis artifacts
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m6-xdos-syscall-boundary-mapping`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

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

## User-Facing Handoff Rule
- The instruction file must be self-sufficient
- If Codex returns a message for the user to forward to Gemini, that message should contain only the instruction file path
- Do not depend on a separate chat message for branch, workflow, or verification details

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- Do not start the next milestone from within this task
