# Gemini Implementation Instruction

## Task ID
20260324-025100-m29e-xdos-write-side-fdc-io-catalog-retry

## Objective
Preserve the accepted raw write-side FDC I/O catalog while removing over-specific `read_path.asm` annotations that exceed the task's conservative wording constraints.

## Branch
- Base: `develop`
- Name: `codex/m29e-xdos-write-side-fdc-io-catalog-retry`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm

## Files To Read First
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Limit changes to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- Do not modify `boot_and_io_notes.md`
- Do not introduce `Graphic RAM`, `buffer`, `hardware behavior`, `OS behavior`, or `port match` wording in `read_path.asm`
- Keep only raw comments that directly match already accepted local observations

## Steps
1. Start from `develop`.
2. Reproduce only the accepted `read_path.asm` comment adjustments for `0xC9EA` and `0xCABA`.
3. Replace the current over-specific comments with conservative raw wording:
   - for `org 0xC9EA`, keep only the direct literal and transfer facts without semantic address-range naming
   - for `org 0xCABA`, keep only the direct transfer fact and omit the `documented port match: none` wording
4. Do not change any other lines.

## Verification
- `git diff -- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

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
- state explicitly that `boot_and_io_notes.md` was left unchanged

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
