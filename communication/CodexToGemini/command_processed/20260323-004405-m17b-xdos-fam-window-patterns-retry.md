# Gemini Implementation Instruction

## Task ID
20260323-004405-m17b-xdos-fam-window-patterns-retry

## Objective
Correct the raw FAM-window pattern task by tightening the wording and unknowns only. Do not expand the helper set and do not broaden the sample set.

## Task Kind
analysis-only

## Slice Rule
This retry is editorial and evidence-tightening only. The raw pattern work is already mostly done. Only fix the notes and README so they stay below the semantic threshold.

## Branch
- Base: `develop`
- Name: `codex/m17b-xdos-fam-window-patterns-retry`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-004139-m17b-xdos-fam-window-patterns-report.md`

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
- Do not add or modify helper scripts in this retry unless absolutely necessary
- Do not change the sampled file set
- Do not claim:
  - `0x1D` reliably points to an FAM offset
  - any packed or traversal semantics
  - any allocation semantics
- Keep allowed relationship labels to:
  - `same`
  - `different`
  - `repeated`
  - `unknown`

## Steps
1. Keep the existing raw pattern section in `boot_and_io_notes.md`, but ensure every sentence stays at raw-pattern level only.
2. If any sentence implies more than raw pattern comparison, downgrade it to `observed` or `unknown`.
3. Update `README.md` only if needed to reflect the actual new boundary:
   - raw FAM windows can now be compared across disks and files
   - semantics remain unknown
4. Do not add new helpers, new sample rows, or new theory.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Acceptance
- The notes section stays at raw-pattern level only
- README reflects the new raw-pattern capability without overclaim
- No helper changes are needed

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
