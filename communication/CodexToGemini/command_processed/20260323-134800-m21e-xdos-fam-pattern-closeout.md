# Gemini Implementation Instruction

## Task ID
20260323-134800-m21e-xdos-fam-pattern-closeout

## Objective
Advance M5 by tightening the FAM pattern closeout boundary using only already observed nibble-safe raw facts, without assigning semantic meaning.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21e-xdos-fam-pattern-closeout`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
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
- Do not create or edit scripts
- Do not propose semantics such as `entry id`, `chain value`, `allocation value`, or `packed map meaning`
- Restrict wording to raw facts already observed:
  - full-sector `0x00..0x0F` range
  - sampled stability
  - raw windows
  - boundary remains unknown

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## FAM Pattern Closeout Boundary (Analysis-Only)`.
2. In that section, summarize only the already observed raw facts:
   - raw windows are cataloged
   - sampled byte/nibble stability is cataloged
   - full-sector range stays within `0x00..0x0F`
   - semantic interpretation remains unknown
3. Update `README.md` by appending one short sentence to the `FAM Window Pattern Semantics` bullet so the same closeout boundary is reflected there.
4. Do not alter any existing tables or evidence notes.

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
- New text is closeout-boundary only
- No FAM semantics are introduced
