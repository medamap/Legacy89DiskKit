# Gemini Implementation Instruction

## Task ID
20260324-030200-m29f-xdos-c9ea-c-register-provenance

## Objective
Catalog the nearest directly observed provenance of register `C` around the already documented `0xC9EA` window so that the unresolved `OUT (C), H` / `IN A, (C)` port can be narrowed by raw evidence rather than semantic guessing.

## Branch
- Base: `develop`
- Name: `codex/m29f-xdos-c9ea-c-register-provenance`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88

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
- Do not upgrade any semantic grade in this task
- Do not introduce `FDC`, `Graphic RAM`, `buffer`, or behavioral role claims unless directly observed in the sampled bytes
- Limit changes to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`

## Steps
1. Starting from the confirmed `0xC9EA` local window, inspect the nearest already known callers and surrounding sampled windows to identify any directly observed operations that set or preserve register `C` before the `OUT (C), H` / `IN A, (C)` instructions.
2. If needed, extend the raw byte window conservatively around the already known write-side area, but only far enough to document direct `ld c,*`, `ld bc,*`, `push/pop bc`, or equivalent provenance-relevant instructions.
3. Add a new section to `boot_and_io_notes.md`:
   - `## C9EA C-Register Provenance Catalog (Analysis-Only)`
   - with columns:
     - `target`
     - `observed provenance fact`
     - `evidence class`
     - `neutral note`
4. Update `read_path.asm` only if the same direct provenance fact should be preserved on an existing `org` block or a newly added directly adjacent raw window block.
5. If the provenance of `C` is still unresolved in the sampled local windows, record that conservatively instead of guessing.

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
- state explicitly whether register `C` provenance became more specific or remained unresolved

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
