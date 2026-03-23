# Gemini Implementation Instruction

## Task ID
20260323-031200-m18a-xdos-shared-placement-catalog

## Objective
Start M2 by building a strict raw catalog of observed shared placement cases in the sampled 2D X-DOS disks, without assigning runtime meaning.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m18a-xdos-shared-placement-catalog`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Existing tracked helper scripts already under `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- You may run existing tracked helper scripts, but do not create new scripts in this task
- Do not assign semantics such as chain, allocation, ownership, or traversal meaning
- Only catalog observed cases where multiple files share the same first observed placement pair or same track-level placement region

## Steps
1. Use existing tracked analysis helpers and current notes to identify representative cases where multiple files appear to begin in the same placement region on the sampled disks.
2. Add a new section to `boot_and_io_notes.md` named `## Observed Shared Placement Cases (Analysis-Only)`.
3. In that section, record a small table with only:
   - disk
   - file A
   - file B (or more, if concise)
   - shared observed placement pair or shared placement region
   - evidence note
4. Keep the section raw and descriptive only.
5. Update `README.md` by appending one short sentence to the `Shared Placement and Runtime Traversal` bullet saying that representative shared-placement cases are now cataloged at the raw observation level.
6. Preserve all existing findings verbatim.

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
- The new section catalogs raw shared-placement observations only
- No new semantic claims are introduced
