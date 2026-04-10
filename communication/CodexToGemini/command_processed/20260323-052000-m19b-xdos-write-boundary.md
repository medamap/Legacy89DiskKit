# Gemini Implementation Instruction

## Task ID
20260323-052000-m19b-xdos-write-boundary

## Objective
Add a write-path boundary section that states what is now cataloged and what remains unknown, without introducing new write semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m19b-xdos-write-boundary`
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
- Do not create scripts
- Do not add new observations
- Do not assign semantics such as rollback, allocation policy, commit order, ownership, or reconstruction algorithm

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Write Path Boundary (Analysis-Only)`.
2. In that section, state only:
   - confirmed write-path entry windows are cataloged
   - write-path helper windows are cataloged
   - detailed FAM/FAT update semantics remain unknown
   - write-side reconstruction ordering remains unknown
   - failure/rollback behavior remains unknown
3. Update `README.md` by appending one short sentence to the `Write-Side Requirements` bullet saying the current write-path boundary remains unresolved beyond the raw catalog.
4. Do not alter any existing catalog table.

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
- New text is boundary-only and non-semantic
