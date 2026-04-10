# Gemini Implementation Instruction

## Task ID
20260323-111800-m21a1-xdos-reconciliation-notes-only

## Objective
Retry M5a with a notes-only implementation reconciliation matrix in `boot_and_io_notes.md`, without touching `README.md`.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21a1-xdos-reconciliation-notes-only`
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
  - `provisional`
  - `unknown`
- Do not use implementation prescriptions
- Do not change existing bullets outside the new section

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Implementation Reconciliation Matrix (Analysis-Only)`.
2. In that section, create a compact table using exactly the required three column names.
3. Use only high-level concerns and raw-analysis boundaries.
4. Do not rewrite any existing section text.

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
- Matrix uses only `confirmed`, `provisional`, or `unknown`
