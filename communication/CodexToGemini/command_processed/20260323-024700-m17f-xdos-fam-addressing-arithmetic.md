# Gemini Implementation Instruction

## Task ID
20260323-024700-m17f-xdos-fam-addressing-arithmetic

## Objective
Document only directly observed addressing arithmetic that may indicate how the reconstructed kernel steps through FAM-adjacent data, without assigning semantic meaning or claiming packed-vs-unpacked interpretation.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m17f-xdos-fam-addressing-arithmetic`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Do not create scripts
- Do not mention write-path helpers
- Do not claim packed, unpacked, chain, allocation, traversal, or field meaning
- Only record directly observed arithmetic or pointer stepping patterns in reconstructed read-path assets

## Steps
1. Inspect `read_path.asm` and existing notes for directly observed arithmetic near FAM-adjacent read-path handling.
2. Record only patterns such as:
   - `inc hl`
   - `dec hl`
   - `add hl, de`
   - `add a, ...`
   - repeated byte loads from neighboring addresses
   if they are directly visible in the currently reconstructed read-path windows.
3. Add one new section to `boot_and_io_notes.md` named `## FAM-Adjacent Addressing Arithmetic (Analysis-Only)`.
4. In that section, list only:
   - observed instruction/address
   - the arithmetic class
   - a short neutral context line
5. Update `README.md` by appending one short sentence to the `FAM Window Pattern Semantics` bullet saying whether FAM-adjacent addressing arithmetic is directly observed or still unknown.
6. Preserve all existing findings in both files.

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
- Existing raw-window/range/read-path-byte findings remain intact
- New wording stays neutral and arithmetic-only
- No semantic claims are introduced
