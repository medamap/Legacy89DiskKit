# Gemini Implementation Instruction

## Task ID
20260324-024200-m29e-xdos-write-side-fdc-io-catalog

## Objective
Catalog directly observed FDC-related I/O patterns in the already documented write-side windows so that write-path evidence can move beyond memory-window-only observations.

## Branch
- Base: `develop`
- Name: `codex/m29e-xdos-write-side-fdc-io-catalog`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/SKILL.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/scripts/xdos_analyze_window.py
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/scripts/z80_disasm_core.py
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/scripts/x1_metadata.json

## Files To Read First
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/SKILL.md

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Use only repository-local evidence for semantic upgrades
- `xdos-semantics-engine` may be used only as a raw annotation helper
- Do not upgrade any semantic grade in this task
- Do not introduce `Graphic RAM`, `buffer`, `OS behavior`, or similar purpose claims
- Limit changes to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Steps
1. Inspect the already documented write-side windows for `sys_wopen_impl`, `sys_wrd_impl`, `helper_c934`, `helper_c938`, `helper_c97e`, `0xC9EA`, and `0xCABA`.
2. Using only directly observed bytes and repo-local helper annotations, identify any directly observed FDC-related I/O patterns such as:
   - direct immediate-port access to `0xFF8`..`0xFFC` or `0xFE8`..`0xFEC`
   - any already reconstructed `IN`/`OUT` instruction whose documented port match is FDC-related
   - any `OUT (C), *` or `IN *, (C)` in the local window where the port cannot be resolved and therefore must remain unresolved
3. Add a new section to `boot_and_io_notes.md`:
   - `## Write-Side FDC I/O Catalog (Analysis-Only)`
   - with columns:
     - `target`
     - `observed I/O pattern`
     - `evidence class`
     - `neutral note`
4. Update `read_path.asm` only if a local comment is needed to preserve the same raw I/O observation on an already existing `org` block.
5. If no direct FDC I/O is visible in the sampled local windows, record that conservatively in the table instead of guessing.

## Verification
- `git diff -- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

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
- state explicitly whether direct sampled FDC I/O was observed or not observed in the local write-side windows

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
