# Gemini Implementation Instruction

## Task ID
20260323-141600-m21f-xdos-write-update-order-catalog

## Objective
Advance M5 by cataloging only the currently observed raw call/order relationships in the write path, without assigning update semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21f-xdos-write-update-order-catalog`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- Do not edit `README.md`
- Preserve all existing findings verbatim
- Do not create or edit scripts
- Do not propose semantics such as `FAT update`, `FAM update`, `rollback`, `allocate`, `write record`, or `commit rule`
- Restrict claims to observed calls, returns, and local ordering only

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Write-Side Update Order Windows (Analysis-Only)`.
2. In that section, create a compact table with:
   - `observed window`
   - `directly observed relation`
   - `evidence class`
3. Limit entries to already reconstructed write-side windows and their immediate call/return order.
4. Use only raw wording such as:
   - `entry window cataloged`
   - `call observed before return`
   - `adjacent helper window observed`
   - `return window observed`
5. Do not introduce any update semantics.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md`

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
- Diff touches only `boot_and_io_notes.md`
- Existing findings remain intact
- New text is raw write-window order catalog only
- No update semantics are introduced
