# Gemini Implementation Instruction

## Task ID
20260323-012946-m17c-xdos-fam-byte-nibble-stability

## Objective
Analyze the sampled raw FAM windows at byte and nibble granularity to determine which positions are stable, variable, or still unknown, without assigning any semantic meaning.

## Task Kind
analysis-only

## Slice Rule
This task is one step before bit-level meaning recovery. Do not interpret any byte or nibble. Only classify observed stability patterns within the already-sampled raw windows.

## Branch
- Base: `develop`
- Name: `codex/m17c-xdos-fam-byte-nibble-stability`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/collect_raw_catalog.py`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/collect_raw_catalog.py`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Do not edit C# production code
- Do not resume implementation work
- Do not add new helper scripts unless absolutely necessary
- Prefer using the existing sampled windows already present in `boot_and_io_notes.md`
- Do not claim:
  - field meaning
  - bit meaning
  - chain meaning
  - allocation meaning
  - traversal meaning
- Allowed position labels are limited to:
  - `stable-byte`
  - `stable-high-nibble`
  - `stable-low-nibble`
  - `variable`
  - `unknown`

## Steps
1. Start from the sampled raw windows already recorded in `boot_and_io_notes.md`.
2. Compare those windows position-by-position:
   - cross-disk same file where data exists
   - intra-disk repeated windows where data exists
3. If helpful, derive a compact table for the first 8 bytes of each sampled window, but do not widen the sample set.
4. Add a new section to `boot_and_io_notes.md` that classifies positions only at byte/nibble stability level.
5. Update `README.md` only if needed to note that stability can now be stated at byte/nibble granularity while semantics remain unknown.
6. Keep every statement below the semantic threshold.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/collect_raw_catalog.py`

## Acceptance
- A tracked notes section exists that classifies sampled positions using only the allowed labels
- No semantic interpretation is introduced
- No helper additions are required unless strictly necessary

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
