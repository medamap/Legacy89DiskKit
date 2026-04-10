# Gemini Implementation Instruction

## Task ID
20260323-023100-m17e1-xdos-fam-readpath-byte-only

## Objective
Replace the previous over-broad kernel-side handling note with a narrower read-path-only statement that documents only directly observed byte consumption of the `0x1D/0x1E` directory pair in `helper_d6af`.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m17e1-xdos-fam-readpath-byte-only`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Do not mention any write-path helper
- Do not mention nibble packing on the write side
- Do not create scripts
- Do not touch any section other than:
  - `## FAM Kernel-Side Value Handling (Analysis-Only)` in `boot_and_io_notes.md`
  - the single sentence in the `FAM Window Pattern Semantics` bullet in `README.md`

## Steps
1. In `boot_and_io_notes.md`, ensure the section `## FAM Kernel-Side Value Handling (Analysis-Only)` exists and contains only read-path evidence.
2. The section must say only:
   - `byte-consume` is directly observed in `helper_d6af` when the directory pair `0x1D/0x1E` is loaded into `D/E`
   - `mask-low-nibble`, `shift-or-rotate`, and write-path handling are `unknown`
3. In `README.md`, revise the one sentence in the `FAM Window Pattern Semantics` bullet so it says kernel-side handling is only directly observed as read-path byte consumption of the directory-linked pair, and everything else remains unknown.
4. Do not add any other interpretation.

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
- No write-path helper names appear in the new/edited text
- Only directly observed read-path byte consumption remains as a positive claim
