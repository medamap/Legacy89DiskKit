# Gemini Implementation Instruction

## Task ID
20260324-031100-m29f-xdos-c9ea-c-register-provenance-retry

## Objective
Preserve the accepted `C9EA C-Register Provenance Catalog` while removing over-specific `read_path.asm` wording that exceeds repository-local conservative analysis rules.

## Branch
- Base: `develop`
- Name: `codex/m29f-xdos-c9ea-c-register-provenance-retry`
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
- Use only repository-local evidence
- Limit changes to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- Do not modify `boot_and_io_notes.md`
- Do not write `port 1A`, `Graphic RAM`, `Blue`, `hardware behavior`, or similar semantic wording in `read_path.asm`
- Keep only directly observed local facts on comments

## Steps
1. Start from `develop`.
2. Keep the idea of adding the `0xC9E1` prolog block if it is directly observed and needed for provenance.
3. In `read_path.asm`, use only conservative raw comments:
   - `org 0xC9E1` may state the observed setup bytes, but must not claim a resolved port identity
   - `org 0xC9EA` must not say `Graphic RAM Blue` or `via C=0x1A`; keep only direct literal / transfer facts already acceptable under repo rules
4. Do not change any other files.

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
