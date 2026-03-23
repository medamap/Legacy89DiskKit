# Gemini Implementation Instruction

## Task ID
20260323-093000-m20d-xdos-early-area-crossdisk-equality

## Objective
Extend M4 by classifying only whether sampled boot and early-area regions are bit-for-bit same or different across the sampled 2D X-DOS disks, without assigning semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m20d-xdos-early-area-crossdisk-equality`
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
- Do not assign semantics such as `required`, `sufficient`, `must`, `boot rule`, `clone rule`, `system file rule`, or `ownership`
- Restrict claims to `same`, `different`, or `unknown` for directly compared raw regions

## Steps
1. Compare only directly observed early-area regions across `XDOS_SYS.D88` and `XDOSUTIL.D88`.
2. Add a new section to `boot_and_io_notes.md` named `## Early-Area Cross-Disk Equality (Analysis-Only)`.
3. In that section, create a compact table with:
   - observed region
   - comparison result
   - evidence note
4. Keep the comparison strictly raw. Acceptable values are:
   - `same`
   - `different`
   - `unknown`
5. Update `README.md` only if needed, by appending one short sentence to the most relevant existing bullet that cross-disk equality for sampled early-area regions is now cataloged at the raw level.
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
- New text is raw comparison catalog only
- No new boot or clone semantics are introduced
