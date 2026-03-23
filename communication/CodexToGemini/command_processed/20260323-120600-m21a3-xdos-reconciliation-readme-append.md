# Gemini Implementation Instruction

## Task ID
20260323-120600-m21a3-xdos-reconciliation-readme-append

## Objective
Append one preserving sentence to `README.md` so the minimal implementation reconciliation matrix is referenced without changing heading structure or existing findings.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21a3-xdos-reconciliation-readme-append`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Preserve all existing findings verbatim
- Do not create or edit scripts
- Do not propose code changes
- Keep all headings unchanged
- Append exactly one sentence to an existing bullet
- Do not replace any line wholesale if a preserving append can be used

## Steps
1. Append one short sentence to the most relevant existing bullet in `README.md`.
2. The sentence should only state that the minimal implementation reconciliation matrix now exists in `boot_and_io_notes.md`.
3. Do not change any headings or existing wording other than that single append.

## Verification
- `git diff -- analysis/xdos-kernel/README.md`

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
- Diff touches only `README.md`
- Heading structure is unchanged
- Only one preserving sentence is appended
