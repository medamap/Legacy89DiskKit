# Gemini Implementation Instruction

## Task ID
20260323-105600-m21a-xdos-implementation-reconciliation-matrix-retry2

## Objective
Retry M5a again with a strict schema for the implementation reconciliation matrix and a preserving append in README.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21a-xdos-implementation-reconciliation-matrix-retry2`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Preserve all existing findings verbatim
- Do not create or edit scripts
- Do not propose code changes
- Keep the `### Critical Unknowns` heading exactly as it is
- The `README.md` change must be a single appended sentence to an existing bullet
- In the matrix, use exactly these column names:
  - `implementation concern`
  - `current evidence grade`
  - `current boundary`
- In the matrix, use only these evidence grades:
  - `confirmed`
  - `provisional`
  - `unknown`
- Do not use `cataloged` as an evidence grade

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Implementation Reconciliation Matrix (Analysis-Only)`.
2. In that section, create a compact table using exactly the required three column names.
3. Keep concerns high level and raw-analysis only.
4. In `README.md`, append one short sentence to the `Write-Side Requirements` bullet or another clearly relevant existing bullet. Do not edit any heading.
5. Do not introduce any new observations or implementation prescriptions.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

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
- `README.md` heading structure is unchanged
- Matrix uses the exact required column names
- Matrix uses only `confirmed`, `provisional`, or `unknown`
