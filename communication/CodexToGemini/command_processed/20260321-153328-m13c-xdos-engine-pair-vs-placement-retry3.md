# Gemini Implementation Instruction

## Task ID
20260321-153328-m13c-xdos-engine-pair-vs-placement-retry3

## Objective
Retry M13c with reproducible evidence. Prove or disprove exact-match correlation between `0x1D/0x1E` and the observed placement pair using tracked analysis assets only.

## Branch
- Base: `develop`
- Name: `codex/m13c-xdos-engine-pair-vs-placement-retry3`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-152321-m13c-xdos-engine-pair-vs-placement-retry2-report.md`

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
- If you use a helper script, it must be committed under `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/`
- The report must reference tracked evidence only; no unsupported private temp scripts
- Do not claim "exact match" unless:
  - the tracked helper asset shows the calculation used
  - the report includes raw output excerpts from that helper
  - the output is sufficient to reconstruct each claimed row

## Steps
1. Create branch `codex/m13c-xdos-engine-pair-vs-placement-retry3` from `develop`.
2. Re-read the current analysis notes for the confirmed entry boundary and `0x1D/0x1E` pair consumption.
3. Add a reproducible tracked helper under `analysis/xdos-kernel/` only if needed.
4. For each sampled file, gather tracked raw evidence that shows:
   - directory `0x1D/0x1E`
   - candidate observed placement pair
   - enough raw bytes or sector header context to justify why that placement was chosen
5. Build a compact evidence table for representative files across both disks showing:
   - filename
   - `0x1D/0x1E`
   - observed placement pair
   - exact match / mismatch / partial correlation
6. State only the strongest defensible conclusions:
   - whether `0x1D/0x1E` equals the observed placement pair for all files, some files, or no files
   - whether there are contradictions across system files and utility files
   - what remains unknown
7. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded section for this comparison.
8. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
9. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13c-xdos-engine-pair-vs-placement-retry3`
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
- explicit raw observation snippets or tracked helper output excerpts that justify the table rows
- explicit note confirming that unrelated local changes were not reset or cleaned
- explicit note listing which prohibited semantic labels were intentionally avoided

## Advancement Rule
- This retry/refinement is allowed automatically because the previous report was not accepted
- Do not start the next milestone from within this task
