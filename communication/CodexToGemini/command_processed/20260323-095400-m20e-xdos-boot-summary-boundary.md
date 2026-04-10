# Gemini Implementation Instruction

## Task ID
20260323-095400-m20e-xdos-boot-summary-boundary

## Objective
Close the current M4 work by summarizing only the established raw boot and early-area observations and the remaining unknowns, without introducing new boot or clone semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m20e-xdos-boot-summary-boundary`
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
- Do not assign semantics such as `required`, `sufficient`, `must`, `boot rule`, `loader rule`, `clone rule`, `system reconstruction rule`, or `ownership`

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Boot And Early-Area Summary Boundary (Analysis-Only)`.
2. In that section, summarize only the already established raw findings:
   - boot and early-area observations are cataloged
   - early-area spans are cataloged
   - cross-disk equality for sampled early-area regions is cataloged
   - some sampled early-area regions are same and some are different
   - the exact boot rule remains unknown
   - the exact required clone reconstruction rule remains unknown
   - the full exact extent of directory and adjacent management regions remains unresolved beyond the observed spans
3. Update `README.md` by appending one short sentence to the most relevant existing bullet to reflect this summary boundary.
4. Do not alter existing tables or evidence notes.

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
- No new boot or clone semantics are introduced
