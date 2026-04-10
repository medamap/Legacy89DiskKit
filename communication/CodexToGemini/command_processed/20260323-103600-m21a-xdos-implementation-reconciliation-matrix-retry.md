# Gemini Implementation Instruction

## Task ID
20260323-103600-m21a-xdos-implementation-reconciliation-matrix-retry

## Objective
Retry M5a by preserving the existing README structure and keeping the implementation reconciliation matrix as a raw analysis-only summary.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21a-xdos-implementation-reconciliation-matrix-retry`
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
- Do not replace or rewrite existing headings in `README.md`
- Keep the `### Critical Unknowns` heading exactly as it is
- The `README.md` change must be a single appended sentence to an existing bullet, not a heading replacement

## Steps
1. Add or preserve a new section in `boot_and_io_notes.md` named `## Implementation Reconciliation Matrix (Analysis-Only)`.
2. In that section, keep only the high-level matrix-style evidence summary.
3. In `README.md`, preserve the existing heading structure exactly and append one short sentence to the most relevant existing bullet instead of editing any heading.
4. Do not introduce any new observations or implementation prescriptions.

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
- New text is matrix-style summary only
