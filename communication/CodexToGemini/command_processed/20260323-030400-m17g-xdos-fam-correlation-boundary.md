# Gemini Implementation Instruction

## Task ID
20260323-030400-m17g-xdos-fam-correlation-boundary

## Objective
Close the current M1 line by documenting the current evidence boundary: raw FAM bytes and read-path directory-pair handling are both observed, but a direct one-to-one mapping between the `0x1D/0x1E` pair and specific raw FAM byte positions is still unproven.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m17g-xdos-fam-correlation-boundary`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Do not create scripts
- Do not add new observations outside currently documented evidence
- Do not claim any direct mapping from `0x1D/0x1E` to a specific raw FAM byte position
- Do not claim chain semantics, allocation semantics, or packed-field meaning

## Steps
1. Add one new section to `boot_and_io_notes.md` named `## FAM Correlation Boundary (Analysis-Only)`.
2. In that section, state only the current evidence boundary:
   - raw FAM windows are observed
   - full-sector 4-bit range is observed
   - read-path byte consumption of `0x1D/0x1E` is observed
   - read-path addressing arithmetic in `helper_d6af` is observed
   - direct correlation from the directory pair to specific raw FAM byte positions remains unproven
3. Update `README.md` by appending one short sentence to the `FAM Window Pattern Semantics` bullet saying that the direct correlation boundary remains unresolved.
4. Preserve all existing findings verbatim.

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
- The new text only summarizes the evidence boundary and unresolved direct correlation
- No new semantic claims are introduced
