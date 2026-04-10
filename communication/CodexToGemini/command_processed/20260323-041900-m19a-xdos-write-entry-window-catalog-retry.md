# Gemini Implementation Instruction

## Task ID
20260323-041900-m19a-xdos-write-entry-window-catalog-retry

## Objective
Correct the previous overclaim by reducing the write-path catalog to raw entry-window facts only.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m19a-xdos-write-entry-window-catalog-retry`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-040800-m19a-xdos-write-entry-window-catalog-report.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Preserve all existing findings verbatim
- Do not create scripts
- Do not use terms such as:
  - update
  - file-open-for-write
  - data-write-from-memory
  - nibble-swapped
  - save path
  - FAM update
  - stack cleanup
  - status return
- Only describe:
  - label or address
  - observed first bytes or `unknown`
  - evidence class
  - neutral note like `documented entry window` or `documented helper window`

## Steps
1. Replace the `## Write Path Entry Windows (Analysis-Only)` section with a stricter raw catalog.
2. Keep each neutral note purely descriptive, for example:
   - `documented entry window`
   - `documented helper window`
   - `documented target address`
3. Keep the `README.md` sentence as a raw observation statement only.
4. Do not change any other sections.

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
- The write-path section is raw catalog only
- No semantic phrasing remains
