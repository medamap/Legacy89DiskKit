# Gemini Implementation Instruction

## Task ID
20260323-122800-m21b-xdos-closeout-boundary

## Objective
Advance M5 by adding a closeout boundary section that states only which areas are now analysis-complete and which areas remain blocked unknowns, without proposing implementation work.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21b-xdos-closeout-boundary`
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
- Do not propose implementation work
- Do not mention specific C# files or methods
- Restrict wording to:
  - `analysis-complete`
  - `blocked unknown`
  - `raw catalog exists`
  - `boundary established`

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Analysis Closeout Boundary (Analysis-Only)`.
2. In that section, summarize only:
   - which major areas now have raw catalogs / boundaries established
   - which major areas remain blocked unknowns
3. Update `README.md` by appending one short sentence to the most relevant existing bullet to note that an analysis closeout boundary now exists.
4. Do not introduce implementation prescriptions or new semantics.

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
- New text is closeout-boundary only
- No implementation prescriptions are introduced
