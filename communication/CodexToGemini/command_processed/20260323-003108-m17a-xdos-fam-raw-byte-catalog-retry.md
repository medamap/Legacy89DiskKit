# Gemini Implementation Instruction

## Task ID
20260323-003108-m17a-xdos-fam-raw-byte-catalog-retry

## Objective
Correct the previous raw-byte catalog task by reducing the analysis back to exact observed bytes, offsets, and windows only, with no FAM chain semantics and no allocation-model language.

## Task Kind
analysis-only

## Slice Rule
This retry is narrower than the original task. Do not expand the sample set. Do not add new semantic claims. Only fix the tracked analysis artifacts so they present reproducible raw evidence and clearly label all meaning as `unknown`.

## Branch
- Base: `develop`
- Name: `codex/m17a-xdos-fam-raw-byte-catalog-retry`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/verification_baseline.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_file_start.py`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260323-000732-m17a-xdos-fam-raw-byte-catalog-report.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Do not edit C# production code
- Do not resume implementation work
- Do not claim or imply:
  - simple track-based allocation chain
  - FAM chain semantics
  - allocation model
  - traversal model
- Keep the helper set minimal and tracked
- If existing tracked helpers from the previous attempt are adequate, reuse them instead of adding more
- Do not leave new temp scripts or untracked evidence helpers behind

## Steps
1. Start from the previous attempt, but strip the notes back to raw observation only.
2. Keep or refine the tracked helpers only if they are needed to reproduce the exact table.
3. In `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`, keep only:
   - disk name
   - filename
   - directory entry base offset
   - directory bytes `0x1A..0x1E`
   - `0x1D/0x1E`
   - first observed placement pair
   - exact FAM-area byte window with byte offsets
4. Replace any interpretation of those FAM bytes with `unknown`.
5. If `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md` mentions stronger conclusions than the raw evidence allows, downgrade them.
6. Use Japanese commit messages if you commit.

## Verification
- `git status --short`
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_file_start.py --help`
- Run the tracked helper or helpers used to generate the catalog and list the exact command lines in the report

## Acceptance
- The tracked notes present exact raw observations only
- Every FAM-related meaning beyond raw bytes is explicitly `unknown`
- No new untracked helper is required
- The report does not overclaim semantics beyond the collected bytes and windows

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
