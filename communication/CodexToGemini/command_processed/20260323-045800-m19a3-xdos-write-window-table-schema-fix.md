# Gemini Implementation Instruction

## Task ID
20260323-045800-m19a3-xdos-write-window-table-schema-fix

## Objective
Fix the write-path window table schema so it contains the required columns and values, while still touching only `boot_and_io_notes.md`.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m19a3-xdos-write-window-table-schema-fix`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-044700-m19a2-xdos-write-window-notes-only-report.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- Do not edit `README.md`
- Preserve all other sections verbatim
- Do not create scripts
- Section must contain exactly one markdown table with these 4 columns:
  - `label or address`
  - `observed bytes`
  - `evidence class`
  - `neutral note`
- Section rows must contain exactly these 5 windows:
  - `sys_wopen_impl`
  - `sys_wrd_impl`
  - `helper_c934`
  - `helper_c938`
  - `helper_c97e`
- `neutral note` values are restricted to:
  - `documented entry window`
  - `documented helper window`
- Do not add prose before or after the table

## Steps
1. Replace the current `## Write Path Entry Windows (Analysis-Only)` section with the required 4-column table only.
2. Fill `observed bytes` from the currently documented confirmed windows.
3. Use `confirmed` as the evidence class for those 5 rows.
4. Do not touch any other section.

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
- Section contains exactly one 4-column table
- The 5 required rows are present
- No prose is added around the table
