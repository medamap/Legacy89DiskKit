# Gemini Implementation Instruction

## Task ID
20260323-025500-m17f1-xdos-readpath-addressing-only

## Objective
Replace the over-broad addressing note with a read-path-only arithmetic note that documents only directly observed pointer/indexing operations in `helper_d6af` and its immediate local window.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m17f1-xdos-readpath-addressing-only`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-024700-m17f-xdos-fam-addressing-arithmetic-report.md`

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
- Do not mention any write-path helper
- Do not mention nibble swap, nibble packing, 4-bit merge, packed data, or allocation engine
- Restrict all positive claims to `helper_d6af` and its immediate local window
- Preserve all existing findings already in the files

## Steps
1. Replace the `## FAM-Adjacent Addressing Arithmetic (Analysis-Only)` section so it contains only read-path arithmetic observed in `helper_d6af` and the directly adjacent local instructions.
2. Allowed observed patterns:
   - `add hl, de`
   - `inc hl`
   - sequential byte loads from adjacent addresses
   - `dec de` only if it is directly present in the same local window and described neutrally
3. Do not mention any address outside the `helper_d6af` local read-path window.
4. Update the extra sentence in the `FAM Window Pattern Semantics` bullet so it says only that read-path addressing arithmetic is directly observed in the reconstructed helper window.
5. Do not modify any other bullets or sections.

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
- No write-path references remain
- No nibble/packing claims remain
- Existing raw-window/range/read-path-byte findings remain intact
