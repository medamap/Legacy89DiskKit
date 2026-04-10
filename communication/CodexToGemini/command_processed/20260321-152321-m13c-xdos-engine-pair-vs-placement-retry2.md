# Gemini Implementation Instruction

## Task ID
20260321-152321-m13c-xdos-engine-pair-vs-placement-retry2

## Objective
Retry M13c with stricter evidence. Determine whether the `0x1D/0x1E` pair exactly matches the observed placement pair for sampled files, and show the raw observations that justify the conclusion.

## Branch
- Base: `develop`
- Name: `codex/m13c-xdos-engine-pair-vs-placement-retry2`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-151232-m13c-xdos-engine-pair-vs-placement-retry.md`

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
- Allowed wording:
  - "observed placement pair"
  - "`0x1D/0x1E` pair"
  - "exact match"
  - "mismatch"
  - "partial correlation"
  - "unknown"
- Do not mention FAM, cluster, physical, logical, drive, side, load address, or entry point
- Do not modify `read_path.asm` or `labels.tsv`
- If you use a helper script, either:
  - commit it under `analysis/xdos-kernel/` if it is part of the lasting analysis asset set, or
  - delete it before finishing if it is only a temporary aid
- Do not claim "exact match" unless the report includes raw supporting observations for each sampled file

## Steps
1. Create branch `codex/m13c-xdos-engine-pair-vs-placement-retry2` from `develop`.
2. Re-read the current analysis notes for the confirmed entry boundary and `0x1D/0x1E` pair consumption.
3. For each sampled file, gather raw evidence that shows:
   - directory `0x1D/0x1E`
   - candidate observed placement pair on the image
   - enough raw bytes or sector header context to justify why that placement was chosen
4. Build a compact evidence table for representative files across both disks showing:
   - filename
   - `0x1D/0x1E`
   - observed placement pair
   - exact match / mismatch / partial correlation
5. State only the strongest defensible conclusions:
   - whether `0x1D/0x1E` equals the observed placement pair for all files, some files, or no files
   - whether there are contradictions across system files and utility files
   - what remains unknown
6. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded section for this comparison.
7. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13c-xdos-engine-pair-vs-placement-retry2`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/README.md analysis/xdos-kernel/boot_and_io_notes.md`
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
- explicit raw observation snippets or tool output excerpts that justify the table rows
- explicit note confirming that unrelated local changes were not reset or cleaned
- explicit note listing which prohibited semantic labels were intentionally avoided

## Advancement Rule
- This retry/refinement is allowed automatically because the previous report was not accepted
- Do not start the next milestone from within this task
