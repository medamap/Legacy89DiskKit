# Gemini Implementation Instruction

## Task ID
20260323-032300-m18b-xdos-shared-placement-boundary

## Objective
Document the current evidence boundary for shared placement: representative raw cases are cataloged, but ownership rules and runtime resolution rules remain unproven.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m18b-xdos-shared-placement-boundary`
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
- Do not claim ownership, priority, allocation, chaining, traversal, or conflict-resolution meaning
- Do not add any new observed cases

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Shared Placement Boundary (Analysis-Only)`.
2. In that section, state only:
   - representative raw shared-placement cases are cataloged
   - shared track-level regions are observed
   - cross-disk same-pair reuse is observed
   - ownership rules remain unknown
   - runtime resolution rules remain unknown
   - write-side reconstruction rules remain unknown
3. Update `README.md` by appending one short sentence to the `Shared Placement and Runtime Traversal` bullet saying the current boundary remains unresolved beyond the raw catalog.
4. Do not alter any existing case tables or findings.

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
- New text stays boundary-only and non-semantic
