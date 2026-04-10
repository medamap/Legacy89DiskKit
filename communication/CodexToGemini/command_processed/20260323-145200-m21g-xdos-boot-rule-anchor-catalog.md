# Gemini Implementation Instruction

## Task ID
20260323-145200-m21g-xdos-boot-rule-anchor-catalog

## Objective
Advance M5 by cataloging only the currently observed raw boot-rule anchors, without asserting any invariant rule.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21g-xdos-boot-rule-anchor-catalog`
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
- Do not assert any invariant boot rule
- Do not use words like `must`, `required`, `sufficient`, `necessary`, or `loader rule`
- Restrict wording to raw anchors already observed:
  - same
  - different
  - observed span
  - observed region
  - boundary remains unknown

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Boot Rule Anchor Catalog (Analysis-Only)`.
2. In that section, summarize only the currently observed raw anchors relevant to boot-related reasoning:
   - Track 0 Head 0 observed span
   - Track 0 Head 1 observed spans and cross-disk same/different split
   - Track 1 Head 0 observed span and cross-disk equality
   - boot rule boundary remains unknown
3. Update `README.md` by appending one short sentence to the most relevant existing bullet to note that boot-rule anchors are now cataloged at the raw level.
4. Do not introduce invariant semantics.

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
- New text is raw boot-anchor catalog only
- No invariant boot semantics are introduced
