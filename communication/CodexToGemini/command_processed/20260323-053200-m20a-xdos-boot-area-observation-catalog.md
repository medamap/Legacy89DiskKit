# Gemini Implementation Instruction

## Task ID
20260323-053200-m20a-xdos-boot-area-observation-catalog

## Objective
Start M4 by cataloging the currently observed boot-area and early management-area facts for the sampled 2D X-DOS disks, without assigning boot semantics beyond direct observation.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m20a-xdos-boot-area-observation-catalog`
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
- You may run existing tracked helpers, but do not create new scripts
- Preserve all existing findings verbatim
- Do not assign boot semantics such as “required”, “sufficient”, “must”, or “loader rule”
- Only catalog directly observed early-area facts

## Steps
1. Inspect the sampled 2D X-DOS disks and current analysis notes for directly observed facts about:
   - boot area presence/copy region
   - early management tracks or sectors
   - where FAT, directory, and FAM are observed
2. Add a new section to `boot_and_io_notes.md` named `## Boot And Early-Area Observations (Analysis-Only)`.
3. In that section, create a compact table with:
   - observed area
   - sampled disks
   - directly observed fact
   - evidence note
4. Update `README.md` by appending one short sentence to the `Geometry Translation Constraints` or other most relevant bullet only if needed to mention that boot/early-area observations are now cataloged at the raw level.
5. Preserve all existing findings verbatim.

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
- No new boot semantics are introduced
