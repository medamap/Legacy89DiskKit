# Gemini Implementation Instruction

## Task ID
20260323-101800-m21a-xdos-implementation-reconciliation-matrix

## Objective
Start M5 by converting the current analysis state into a strictly evidence-graded implementation reconciliation matrix, without proposing code changes yet.

## Task Kind
- Investigation

## Branch
- Base: `develop`
- Name: `codex/m21a-xdos-implementation-reconciliation-matrix`
- Gemini may commit on this branch for tracked analysis-note updates only
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Constraints
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- Follow `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`
- Edit only:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- Preserve all existing findings verbatim
- Do not create or edit scripts
- Do not propose code changes
- Do not name specific C# methods or files outside the two analysis documents
- Restrict statements to evidence grades such as:
  - `confirmed`
  - `provisional`
  - `unknown`

## Steps
1. Add a new section to `boot_and_io_notes.md` named `## Implementation Reconciliation Matrix (Analysis-Only)`.
2. In that section, create a compact table with:
   - implementation concern
   - current evidence grade
   - current boundary
3. Keep the concerns high level, for example:
   - read-path traversal inputs
   - write-path entry windows
   - shared placement raw pattern
   - boot and early-area observations
   - exact reconstruction rule
4. Update `README.md` by appending one short sentence to the most relevant existing bullet that an implementation reconciliation matrix now exists at the raw analysis level.
5. Do not introduce any new observations or implementation prescriptions.

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
- New text is matrix-style summary only
- No code prescriptions or new semantics are introduced
