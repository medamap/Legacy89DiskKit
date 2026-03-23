# Gemini Implementation Instruction

## Task ID
20260323-040800-m19a-xdos-write-entry-window-catalog

## Objective
Start M3 by cataloging the directly reconstructed write-path entry windows and immediate helper windows, without assigning write semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m19a-xdos-write-entry-window-catalog`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Do not create scripts
- Do not assign semantics such as allocation, rollback, commit order, or reconstruction rules
- Only catalog directly reconstructed write-path entry windows and immediate helper windows already present in current assets

## Steps
1. Inspect the currently reconstructed write-path related labels and windows already documented in the analysis assets.
2. Add a new section to `boot_and_io_notes.md` named `## Write Path Entry Windows (Analysis-Only)`.
3. In that section, create a compact table with:
   - label or address
   - observed first bytes / window note
   - evidence class (`direct-byte`, `reconstructed-window`, or `unknown`)
   - neutral note
4. Update `README.md` by appending one short sentence to the `Write-Side Requirements` bullet saying that write-path entry windows are now cataloged at the raw observation level.
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
- New text catalogs windows only
- No new semantic claims are introduced
