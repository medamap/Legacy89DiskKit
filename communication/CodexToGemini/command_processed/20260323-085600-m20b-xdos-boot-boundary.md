# Gemini Implementation Instruction

## Task ID
20260323-085600-m20b-xdos-boot-boundary

## Objective
Continue M4 by adding only the current boundary for boot and early-area understanding, based strictly on the raw observation catalog that now exists.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m20b-xdos-boot-boundary`
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
- Do not add new observations
- Do not assign semantics such as `required`, `sufficient`, `must`, `boot rule`, `loader rule`, or `clone rule`
- Do not claim exact management-area size beyond what is already directly observed

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Boot And Early-Area Boundary (Analysis-Only)`.
2. In that section, summarize only the already established raw facts:
   - boot and early-area observations are cataloged
   - volume record, FAT area, directory start, FAM area, and boot copy region are observed at raw locations
   - the exact boot rule and exact required clone reconstruction conditions remain unknown
   - the exact full extent of the directory area remains unknown beyond the directly observed start
3. Update `README.md` by appending one short sentence to the most relevant existing bullet so the same boundary is reflected there.
4. Do not alter any existing tables or evidence notes.

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
- New text is summary-boundary only
- No new boot semantics are introduced
