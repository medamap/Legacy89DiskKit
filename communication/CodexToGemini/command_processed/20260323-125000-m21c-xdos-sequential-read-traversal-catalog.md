# Gemini Implementation Instruction

## Task ID
20260323-125000-m21c-xdos-sequential-read-traversal-catalog

## Objective
Advance M5 by cataloging only the currently observed downstream read-traversal windows after the initial placement pair is consumed, without assigning traversal semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21c-xdos-sequential-read-traversal-catalog`
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
- Do not edit `README.md`
- Preserve all existing findings verbatim
- Do not create or edit scripts
- Do not propose semantics such as `next cluster`, `continuation rule`, `record rule`, or `EOF rule`
- Restrict claims to observed windows, addresses, and direct control-transfer relationships

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Downstream Read Traversal Windows (Analysis-Only)`.
2. In that section, create a compact table with:
   - `observed window`
   - `directly observed relation`
   - `evidence class`
3. Limit entries to already reconstructed downstream read-path windows after the initial placement-pair handling area.
4. Use only raw wording such as:
   - `jp target observed`
   - `downstream window cataloged`
   - `control transfer observed`
5. Do not introduce traversal meaning beyond those direct observations.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md`

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
- Diff touches only `boot_and_io_notes.md`
- Existing findings remain intact
- New text is raw downstream-window catalog only
- No traversal semantics are introduced
