# Gemini Implementation Instruction

## Task ID
20260321-154035-m13c1-xdos-observed-placement-detection

## Objective
Define and prove a directory-independent method for identifying the initial observed placement pair of sampled files on X-DOS disk images.

## Branch
- Base: `develop`
- Name: `codex/m13c1-xdos-observed-placement-detection`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-153328-m13c-xdos-engine-pair-vs-placement-retry3-report.md`

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
- Do not inspect or use directory bytes `0x1D/0x1E` in this task
- Do not compare any observed placement result to directory fields in this task
- Allowed wording:
  - "observed placement pair"
  - "candidate detection method"
  - "reproducible detection"
  - "unknown"
- Do not mention FAM, cluster, logical, load address, entry point
- If you use a helper script, it must be committed under `analysis/xdos-kernel/`

## Steps
1. Create branch `codex/m13c1-xdos-observed-placement-detection` from `develop`.
2. Choose sampled files that appear in the images and whose initial content can be independently recognized.
3. Define a reproducible method for locating the initial observed placement pair without using directory fields.
4. The method must be evidenced by tracked assets only:
   - tracked helper script under `analysis/xdos-kernel/`, or
   - tracked raw observation log / table
5. For each sampled file, report:
   - how the file start was recognized independently
   - the resulting observed placement pair
   - enough raw output to reproduce the claim
6. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded section for the detection method and sampled results.
7. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13c1-xdos-observed-placement-detection`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/`
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
- explicit raw observation snippets or tracked helper output excerpts that justify each sampled placement pair
- explicit note confirming that unrelated local changes were not reset or cleaned
- explicit note stating that no directory field comparison was performed in this task

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed until the remaining analysis reaches 100%
- Do not start the next milestone from within this task
