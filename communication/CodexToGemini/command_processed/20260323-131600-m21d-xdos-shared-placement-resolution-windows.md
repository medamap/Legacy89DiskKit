# Gemini Implementation Instruction

## Task ID
20260323-131600-m21d-xdos-shared-placement-resolution-windows

## Objective
Advance M5 by cataloging only the currently observed windows that may participate in shared-placement resolution, without assigning resolution semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21d-xdos-shared-placement-resolution-windows`
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
- Do not propose semantics such as `shared-owner`, `allocation rule`, `collision rule`, or `resolution rule`
- Restrict claims to observed windows, addresses, and direct control transfers only

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Shared Placement Resolution Windows (Analysis-Only)`.
2. In that section, create a compact table with:
   - `observed window`
   - `directly observed relation`
   - `evidence class`
3. Limit entries to windows already present in the reconstructed read path that may be relevant after the shared-placement raw pattern is observed.
4. Use only raw wording such as:
   - `window cataloged`
   - `call target observed`
   - `jp target observed`
   - `adjacent control transfer observed`
5. Do not introduce any semantic resolution meaning.

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
- New text is raw window catalog only
- No resolution semantics are introduced
