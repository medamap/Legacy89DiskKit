# Gemini Implementation Instruction

## Task ID
20260323-035600-m18e-xdos-shared-placement-summary-boundary

## Objective
Close the current shared-placement milestone by summarizing only the established raw observation pattern and the remaining unknowns, without introducing new evidence or semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m18e-xdos-shared-placement-summary-boundary`
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
- Do not assign semantics
- Do not claim ownership, conflict resolution, traversal, or write-side rules

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Shared Placement Summary Boundary (Analysis-Only)`.
2. In that section, summarize only the already established raw observations:
   - representative same-disk shared track-level cases are cataloged
   - same-disk exact pair duplication is not observed for valid files
   - cataloged shared-track cases currently fit `same-1D-different-1E`
   - ownership/runtime/write reconstruction rules remain unknown
3. Update `README.md` by appending one short sentence to the `Shared Placement and Runtime Traversal` bullet that reflects this summary boundary.
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
- No new semantic claims are introduced
