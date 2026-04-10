# Gemini Implementation Instruction

## Task ID
20260323-012513-m17b-xdos-fam-window-patterns-retry3

## Objective
Apply a fixed wording correction for the raw FAM-window section and the README bullet, with no other edits.

## Task Kind
analysis-only

## Slice Rule
This retry is literal-text correction. Do not reinterpret the task. Do not tighten or soften any other section. Edit only the exact lines listed below.

## Branch
- Base: `develop`
- Name: `codex/m17b-xdos-fam-window-patterns-retry3`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-012248-m17b-xdos-fam-window-patterns-retry2-report.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Do not change any helper script
- Do not add any file
- Do not edit any section outside:
  - `## Raw FAM Window Patterns (Analysis-Only)` in `boot_and_io_notes.md`
  - the single `**FAM Window Pattern Semantics**` bullet in `README.md`
- In the raw FAM section and README bullet, do not use or imply:
  - `points to`
  - `allocation chain`
  - `traversal`
  - `packed`
  - `bitmask`
  - `linear chain`

## Steps
1. If `## Raw FAM Window Patterns (Analysis-Only)` is absent on `develop`, append that section at the end of `boot_and_io_notes.md`.
2. Use the existing tables exactly as they appeared in the previous attempt.
3. Set the summary prose in that section to exactly these four bullets:
   - `- **Cross-disk identity**: \`X-DOS System\` shows the same 8-byte raw window across the two sampled disks.`
   - `- **Window repetition**: identical 8-byte windows are observed for more than one sampled file or more than one sampled offset.`
   - `- **Offset-local observation**: the compared raw windows are collected from the FAM area using the sampled file rows above; no further semantics are claimed here.`
   - `- **Status**: the meaning of these windows is **unknown**.`
4. In `README.md`, ensure the `**FAM Window Pattern Semantics**` bullet is exactly:
   - `- **FAM Window Pattern Semantics**: Raw 8-byte windows can be compared across sampled disks and sampled files. Some sampled windows are the same, some are different, and some are repeated. The meaning of these windows remains unknown.`
5. Do not modify any other bullet or any other section.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Acceptance
- The diff only touches the raw FAM-window section and the one README bullet
- The raw FAM-window section contains the exact four bullets above
- The README contains the exact bullet above
- No helper changes

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
