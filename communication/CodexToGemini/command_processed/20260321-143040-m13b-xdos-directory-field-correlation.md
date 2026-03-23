# Gemini Implementation Instruction

## Task ID
20260321-143040-m13b-xdos-directory-field-correlation

## Objective
Using the now-confirmed 32-byte entry boundary, determine the strongest defensible correlations for directory bytes `0x1B/0x1C` and `0x1D/0x1E` without overnaming them.

## Branch
- Base: `develop`
- Name: `codex/m13b-xdos-directory-field-correlation`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-142332-m13a-xdos-directory-entry-boundary-retry-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-135131-m13-xdos-dir-byte-placement-correlation-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-140012-m13-xdos-dir-byte-placement-correlation-report.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-140734-m13-xdos-dir-byte-placement-correlation-report-retry2.md`

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
- Use the confirmed entry-base convention from M13a
- Do not call any field "FAM", "cluster", "physical", "logical", "drive", or "side" unless directly proven
- Allowed wording:
  - "track-like / sector-like pair"
  - "observed content-start correlation"
  - "bytes consumed by helper_d6af"
  - "engine-input pair"
  - "unknown"
- You may update `read_path.asm` only to weaken wording, not to add new semantics

## Steps
1. Create branch `codex/m13b-xdos-directory-field-correlation` from `develop`.
2. Re-read M13a so the byte indices are counted from the correct entry base.
3. Build a compact evidence table for representative files across both disks showing:
   - filename
   - `0x1B/0x1C`
   - `0x1D/0x1E`
   - observed content-start bytes/position in the image
4. From that table, derive only the strongest defensible statements:
   - whether `0x1B/0x1C` correlate with an observed content-start pair
   - whether `0x1D/0x1E` correlate with an observed content-start pair
   - whether `0x1D/0x1E` are directly consumed by `helper_d6af`
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded section that clearly separates:
   - direct observation
   - correlation
   - contradiction
   - unknown
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13b-xdos-directory-field-correlation`
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
- explicit note listing which terms were intentionally avoided because they remain unproven

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed until the remaining analysis reaches 100%
- Do not start the next milestone from within this task
