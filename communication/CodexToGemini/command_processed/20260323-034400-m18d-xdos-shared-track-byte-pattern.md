# Gemini Implementation Instruction

## Task ID
20260323-034400-m18d-xdos-shared-track-byte-pattern

## Objective
Check, at the raw observation level only, whether the currently cataloged same-disk shared track-level cases are characterized by a common first byte (`0x1D`) with differing second byte (`0x1E`), without assigning semantics.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m18d-xdos-shared-track-byte-pattern`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- existing tracked helpers under `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- You may run existing tracked helper scripts, but do not create new scripts
- Do not assign semantics
- Do not infer ownership, chaining, allocation, or runtime resolution
- Do not edit or commit any helper script changes
- If local helper scripts become modified during investigation, do not include them in the commit

## Steps
1. Reuse existing tracked helpers to inspect the same-disk shared track-level cases already cataloged in `boot_and_io_notes.md`.
2. For those cases only, determine whether the raw pattern is:
   - same `0x1D`, different `0x1E`
   - mixed
   - unknown
3. Add a new section to `boot_and_io_notes.md` named `## Shared Track Byte Pattern Check (Analysis-Only)`.
4. In that section, record a small table with:
   - disk
   - representative files
   - result (`same-1D-different-1E`, `mixed`, or `unknown`)
   - evidence note
5. Update `README.md` by appending one short sentence to the `Shared Placement and Runtime Traversal` bullet reflecting the result at this raw-observation level.
6. Preserve all existing findings verbatim.

## Verification
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
- `git diff --name-only`

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
- The new text answers only the raw shared-track byte-pattern question
- No new semantic claims are introduced
