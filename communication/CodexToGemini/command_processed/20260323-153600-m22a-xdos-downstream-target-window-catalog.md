# Gemini Implementation Instruction

## Task ID
20260323-153600-m22a-xdos-downstream-target-window-catalog

## Objective
Start the extended full-understanding phase by cataloging raw byte windows for the downstream read targets already observed from `helper_d6af`, without assigning semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m22a-xdos-downstream-target-window-catalog`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- Preserve all existing findings verbatim
- Do not create or edit scripts
- Do not edit `README.md`
- Do not assign semantics such as `next record`, `continuation`, `chain rule`, `EOF`, or `shared resolution`
- Restrict claims to raw byte windows, labels, and direct control-transfer relations only

## Steps
1. Identify the already observed downstream targets reached from `helper_d6af`.
2. Add a new section to `boot_and_io_notes.md` named `## Downstream Target Byte Windows (Analysis-Only)`.
3. In that section, create a compact table with:
   - `target`
   - `observed bytes`
   - `evidence class`
   - `neutral note`
4. If `read_path.asm` does not already contain compact raw windows for those targets, add conservative raw byte windows only for directly observed target entry ranges.
5. Use only wording such as:
   - `target window cataloged`
   - `entry bytes observed`
   - `direct jump/call target`
6. Do not introduce target semantics.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/read_path.asm`

## Deliverable
- Markdown report in `communication/GeminiToCodex/report_waiting/`

## Report Requirements
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

## Acceptance Criteria
- Diff touches only the two target files
- Existing findings remain intact
- New text is raw target-window catalog only
- No traversal semantics are introduced
