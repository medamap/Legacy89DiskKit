# Gemini Implementation Instruction

## Task ID
20260323-024000-m17e1-xdos-fam-readpath-byte-only-retry

## Objective
Correct the previous README regression while keeping the read-path-only scope. Preserve all previously established raw-window and 4-bit-range findings, and only append the new read-path byte-consume sentence.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m17e1-xdos-fam-readpath-byte-only-retry`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-023100-m17e1-xdos-fam-readpath-byte-only.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Preserve the existing raw-window findings in `README.md`
- Preserve the existing 4-bit-range finding in `README.md`
- Only append one final sentence about read-path byte consumption
- Do not mention any write-path helper
- Do not create scripts

## Steps
1. In `boot_and_io_notes.md`, keep the `## FAM Kernel-Side Value Handling (Analysis-Only)` section limited to:
   - `byte-consume` directly observed in `helper_d6af` for the directory pair `0x1D/0x1E`
   - everything else unknown
2. In `README.md`, keep the existing `FAM Window Pattern Semantics` bullet body intact up to the current statement about the full 512-byte FAM sector staying in `0x00..0x0F` with max `0x0A`.
3. Append exactly one additional sentence to that same bullet explaining that kernel-side handling is only directly observed as read-path byte consumption of the directory-linked pair, and everything else remains unknown.
4. Do not replace the whole bullet.

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
- `README.md` still contains the raw-window and 4-bit-range findings
- Only one new sentence is appended to the `FAM Window Pattern Semantics` bullet
- No write-path helper names appear
