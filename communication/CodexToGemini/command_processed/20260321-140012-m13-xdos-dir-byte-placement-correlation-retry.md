# Gemini Implementation Instruction

## Task ID
20260321-140012-m13-xdos-dir-byte-placement-correlation-retry

## Objective
Retry the M13 directory-byte analysis with stricter evidence grading. The goal is to prove only what the current artifacts directly support about directory bytes `0x1D` and `0x1E`.

## Branch
- Base: `develop`
- Name: `codex/m13-xdos-dir-byte-placement-correlation-retry`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-135131-m13-xdos-dir-byte-placement-correlation-report.md`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- This is an analysis-only task; do not modify C# or C++ product code
- Do not reset, stash, revert, or otherwise clean unrelated local changes
- Ignore unrelated modified or untracked files unless they block your target files
- The previous attempt overstated the claim. Do not repeat that error.
- Do not claim `0x1D/0x1E` are physical track/sector unless directly proven from D88 structure plus kernel behavior
- Do not claim overlapping files unless you can show the occupied ranges and prove they overlap
- Do not insert explanatory comments into `read_path.asm` that state unproven semantics
- Keep all changes conservative and evidence-graded
- Distinguish explicitly between:
  - direct-byte observations
  - image-level correlations
  - plausible interpretations
  - unknown

## Steps
1. Create branch `codex/m13-xdos-dir-byte-placement-correlation-retry` from `develop`.
2. Read the failed report and identify exactly which claims were too strong.
3. Re-run the correlation analysis for representative files on `XDOS_SYS.D88` and `XDOSUTIL.D88`.
4. Determine the strongest defensible statement for bytes `0x1D` and `0x1E`. Examples of acceptable outcomes:
   - they correlate with a starting logical position
   - they correlate with a starting track-like and sector-like pair
   - they do not yet prove physical D88 track/sector identity
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with a revised section that clearly labels:
   - direct observations
   - correlations
   - remaining ambiguity
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording changes.
7. Leave `analysis/xdos-kernel/read_path.asm` unchanged unless you can make a wording change that removes overclaiming without adding new unsupported meaning.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13-xdos-dir-byte-placement-correlation-retry`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/README.md analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/boot_and_io_notes.md`
- `git status --short`

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
- explicit note confirming that unrelated local changes were not reset or cleaned
- explicit note stating which prior claims were downgraded and why

## Advancement Rule
- This retry is allowed automatically because the previous report was not accepted
- Do not start the next milestone from within this task
