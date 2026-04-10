# Gemini Implementation Instruction

## Task ID
20260323-050900-m19a4-xdos-write-readme-append-only

## Objective
Append one preserving sentence to the `Write-Side Requirements` bullet in `README.md` so it reflects the newly cataloged raw write-path windows without removing existing findings.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m19a4-xdos-write-readme-append-only`
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
- Do not edit `boot_and_io_notes.md`
- Preserve the full existing `Write-Side Requirements` bullet text verbatim
- Only append one new sentence at the end of that bullet
- New sentence must be raw-observation wording only:
  - `Confirmed write-path entry windows are now cataloged at the raw observation level.`
- Do not change any other bullets

## Steps
1. Locate the `Write-Side Requirements` bullet in `README.md`.
2. Keep all current sentences intact.
3. Append exactly one sentence to the end:
   `Confirmed write-path entry windows are now cataloged at the raw observation level.`
4. Do not modify any other content.

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
- Diff touches only `analysis/xdos-kernel/README.md`
- Existing bullet text is preserved
- Exactly one sentence is appended
