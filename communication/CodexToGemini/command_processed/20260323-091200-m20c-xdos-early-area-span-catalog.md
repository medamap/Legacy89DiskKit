# Gemini Implementation Instruction

## Task ID
20260323-091200-m20c-xdos-early-area-span-catalog

## Objective
Extend M4 by cataloging only the directly observed early-area sector spans on the sampled 2D X-DOS disks, without assigning boot or clone semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m20c-xdos-early-area-span-catalog`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
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
- You may use existing tracked helpers, but do not create or edit scripts
- Do not assign semantics such as `required`, `sufficient`, `must`, `boot rule`, `loader rule`, `clone rule`, or `ownership`
- Do not claim that an observed span is the complete required extent unless that is already directly proven

## Steps
1. Inspect the sampled 2D X-DOS disks only for raw early-area sector span observations around:
   - Track 0 Head 0
   - Track 0 Head 1
   - Track 1 Head 0
2. Add a new section to `boot_and_io_notes.md` named `## Early-Area Span Catalog (Analysis-Only)`.
3. In that section, create a compact table with:
   - observed region
   - sampled disks
   - directly observed span
   - evidence note
4. Keep the table raw and descriptive only. Examples of acceptable wording:
   - `observed occupied span`
   - `observed continuous sector span`
   - `observed first confirmed sector`
5. Update `README.md` only if needed, by appending one short sentence to the most relevant existing bullet that early-area spans are now cataloged at the raw level.
6. Do not alter any existing tables or evidence notes.

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
- New text is raw observation catalog only
- No new boot or clone semantics are introduced
