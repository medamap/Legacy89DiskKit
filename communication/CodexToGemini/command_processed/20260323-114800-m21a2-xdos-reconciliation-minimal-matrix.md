# Gemini Implementation Instruction

## Task ID
20260323-114800-m21a2-xdos-reconciliation-minimal-matrix

## Objective
Add a minimal implementation reconciliation matrix to `boot_and_io_notes.md` using only conservative raw-analysis statements.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21a2-xdos-reconciliation-minimal-matrix`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- Do not edit `README.md`
- Preserve all existing findings verbatim
- Do not create or edit scripts
- Do not propose code changes
- Use exactly these column names:
  - `implementation concern`
  - `current evidence grade`
  - `current boundary`
- Use only these evidence grades:
  - `confirmed`
  - `unknown`
- Do not use `provisional`
- Use at most 6 rows
- Do not use words like `required`, `must`, `sufficient`, `imply`, `likely`
- Keep every row phrased as raw-analysis boundary only

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Implementation Reconciliation Matrix (Analysis-Only)`.
2. Create a compact table with at most 6 rows.
3. Keep confirmed rows limited to already cataloged or directly observed raw facts.
4. Keep unknown rows limited to still-unresolved boundaries.

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
- Matrix uses the exact required column names
- Matrix uses only `confirmed` or `unknown`
- Matrix contains at most 6 rows
