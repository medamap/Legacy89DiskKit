# Gemini Implementation Instruction

## Task ID
20260321-135131-m13-xdos-dir-byte-placement-correlation

## Objective
Determine whether X-DOS directory byte `0x1D` correlates with per-file starting position, packed placement, or another observable layout property on real 2D system disks.

## Branch
- Base: `develop`
- Name: `codex/m13-xdos-dir-byte-placement-correlation`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-131911-m12-xdos-boot-clone-conditions-report.md`

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
- Do not invent bytes, fabricated sector maps, or guessed field meanings
- Keep all claims evidence-graded
- Distinguish explicitly between:
  - direct-byte observations
  - image-level correlations
  - hypotheses
  - unknown
- Scope is limited to directory entry byte `0x1D`, file placement correlation, and shared placement evidence
- Do not claim bit-level FAM packing is solved unless directly demonstrated
- Do not claim full bootability rules in this task

## Steps
1. Create branch `codex/m13-xdos-dir-byte-placement-correlation` from `develop`.
2. Re-read the current analysis notes to extract the current claim set around:
   - directory offset `0x1D`
   - `FirstCluster`
   - packed/shared placement on dense 2D images
3. Inspect `XDOS_SYS.D88` and `XDOSUTIL.D88` directly and build a small evidence table for representative files:
   - at least one small utility file
   - at least one larger system file
   - at least one case where multiple files appear to share a logical track/cluster
4. For each representative file, correlate as far as directly possible:
   - directory entry values
   - size
   - starting cluster
   - byte `0x1D`
   - observed physical or logical sector occupancy
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with a dedicated section for directory-byte / placement correlation.
6. Update `analysis/xdos-kernel/README.md` if the project-wide status or critical unknown list changes.
7. Update `analysis/xdos-kernel/labels.tsv` or `analysis/xdos-kernel/read_path.asm` only if a directly supported label clarification is necessary.
8. Keep conclusions conservative:
   - if `0x1D` looks like a starting logical record, say why
   - if `0x1D` looks like a packed sub-track start, say why
   - if multiple explanations remain viable, mark them as competing hypotheses
9. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m13-xdos-dir-byte-placement-correlation`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/README.md analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/boot_and_io_notes.md`
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
- explicit note stating what is direct observation vs correlation vs hypothesis

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- Do not start the next milestone from within this task
