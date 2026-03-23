# Gemini Implementation Instruction

## Task ID
20260323-012248-m17b-xdos-fam-window-patterns-retry2

## Objective
Fix only the raw FAM-window wording in the dedicated section and the single README unknown entry, without touching any other section.

## Task Kind
analysis-only

## Slice Rule
This retry is extremely narrow. Edit only one section in `boot_and_io_notes.md` and one bullet in `README.md`. Do not touch read-path hypotheses, write-path hypotheses, geometry text, helper scripts, or sampled file rows outside the raw FAM-window section.

## Branch
- Base: `develop`
- Name: `codex/m17b-xdos-fam-window-patterns-retry2`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-004405-m17b-xdos-fam-window-patterns-retry-report.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Do not change any helper script
- Do not add any file
- Do not edit any section outside:
  - `## Raw FAM Window Patterns (Analysis-Only)` in `boot_and_io_notes.md`
  - the single `**FAM Window Pattern Semantics**` bullet in `README.md`
- Do not use or imply:
  - `0x1D` reliably points to an FAM offset
  - allocation chain
  - traversal semantics
  - packed meaning
  - bit meaning

## Steps
1. In `boot_and_io_notes.md`, edit only the `## Raw FAM Window Patterns (Analysis-Only)` section.
2. Keep the existing tables, but tighten only the prose lines around them.
3. Replace any remaining stronger phrases with wording at raw-pattern level only, such as:
   - `same`
   - `different`
   - `repeated`
   - `observed alongside`
   - `unknown`
4. In `README.md`, edit only the `**FAM Window Pattern Semantics**` bullet so it states:
   - raw windows can be compared
   - some are stable across disks
   - semantics remain unknown
5. Do not touch any other bullet in `README.md`.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Acceptance
- The diff only touches the raw FAM-window section and the one README bullet
- The raw FAM-window section stays at same/different/repeated/unknown level
- No helper changes
- No wording about allocation chain or traversal remains in that section or in the README bullet

## Deliverable
- Markdown report in `communication/GeminiToCodex/report_waiting/`

## Report Requirements
- task id
- instruction filename
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

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
