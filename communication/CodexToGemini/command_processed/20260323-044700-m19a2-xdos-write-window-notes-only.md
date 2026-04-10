# Gemini Implementation Instruction

## Task ID
20260323-044700-m19a2-xdos-write-window-notes-only

## Objective
Replace the old write-path specification in `boot_and_io_notes.md` with a 5-row raw window catalog only, without touching `README.md`.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m19a2-xdos-write-window-notes-only`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- Do not edit `README.md`
- Preserve all other sections verbatim
- Do not create scripts
- Only catalog these 5 confirmed windows:
  - `sys_wopen_impl`
  - `sys_wrd_impl`
  - `helper_c934`
  - `helper_c938`
  - `helper_c97e`
- Neutral notes are restricted to:
  - `documented entry window`
  - `documented helper window`

## Steps
1. Replace the existing write-path specification section in `boot_and_io_notes.md` with `## Write Path Entry Windows (Analysis-Only)`.
2. That section must contain exactly one 5-row table for the confirmed windows above.
3. Do not add any prose other than the section header and table.
4. Do not touch `README.md`.

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
- Diff touches only `analysis/xdos-kernel/boot_and_io_notes.md`
- The section contains exactly the 5 confirmed windows
- No semantic phrasing remains
