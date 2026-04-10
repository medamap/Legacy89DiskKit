# Gemini Implementation Instruction

## Task ID
20260323-033100-m18c-xdos-exact-pair-duplication-check

## Objective
Determine, at the raw observation level only, whether different files on the same sampled disk ever share the exact same first observed placement pair, and document the result without assigning meaning.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m18c-xdos-exact-pair-duplication-check`
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
- Do not infer ownership, conflict resolution, or runtime rules
- Only answer the narrow question: same-disk exact pair duplication observed / not observed / unknown

## Steps
1. Reuse existing tracked raw-analysis helpers to inspect the first observed placement pairs on `XDOS_SYS.D88` and `XDOSUTIL.D88`.
2. Determine whether any two different files on the same disk share the exact same first observed placement pair.
3. Add a new section to `boot_and_io_notes.md` named `## Exact Pair Duplication Check (Analysis-Only)`.
4. In that section, record:
   - disk
   - result: `observed`, `not observed`, or `unknown`
   - a short evidence note
5. Update `README.md` by appending one short sentence to the `Shared Placement and Runtime Traversal` bullet reflecting this exact-pair duplication check.
6. Preserve all existing findings verbatim.

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
- The new text answers only the exact-pair duplication check
- No new semantic claims are introduced
