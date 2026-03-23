# Gemini Implementation Instruction

## Task ID
20260323-043000-m19a1-xdos-write-confirmed-window-catalog

## Objective
Catalog only the currently confirmed write-path windows with direct observed bytes, and leave unknown targets out of scope.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m19a1-xdos-write-confirmed-window-catalog`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-041900-m19a-xdos-write-entry-window-catalog-retry.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Preserve all existing findings verbatim
- Do not create scripts
- Do not mention:
  - `sys_devo_impl`
  - `sys_load_impl`
- Do not use semantic phrases like `update`, `save`, `cleanup`, `status return`, `file-open-for-write`, `data-write-from-memory`
- Only catalog these 5 confirmed windows:
  - `sys_wopen_impl`
  - `sys_wrd_impl`
  - `helper_c934`
  - `helper_c938`
  - `helper_c97e`

## Steps
1. Replace the `## Write Path Entry Windows (Analysis-Only)` section with a compact 5-row catalog only.
2. Each row must contain:
   - label or address
   - observed bytes
   - evidence class
   - neutral note: either `documented entry window` or `documented helper window`
3. Update the `README.md` `Write-Side Requirements` bullet with one short raw statement that confirmed write-path windows are cataloged at the raw observation level.
4. Do not change any other text.

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
- The write-path section contains exactly the 5 confirmed windows
- No semantic phrasing remains
