# Gemini Implementation Instruction

## Task ID
20260322-222754-m15-xdos-bootable-clone-reconciliation

## Objective
Re-evaluate the bootable 2D X-DOS clone conditions using only the currently tracked analysis assets, and separate what is now proven from what remains unknown before implementation work begins.

## Branch
- Base: `develop`
- Name: `codex/m15-xdos-bootable-clone-reconciliation`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/find_file_start.py`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

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
- Keep all new helper logic under `analysis/xdos-kernel/` if helper changes are necessary
- Do not leave new untracked helper files outside `analysis/xdos-kernel/`
- Be explicit about evidence class
- If a claim depends on extrapolation beyond sampled files, mark it as `unknown` or `provisional`

## Steps
1. Create branch `codex/m15-xdos-bootable-clone-reconciliation` from `develop`.
2. Read the current tracked analysis and restate the current confirmed facts relevant to cloning:
   - directory entry boundary
   - `0x1D/0x1E` pair handling
   - observed placement detection
   - geometry translation `(C * 2 + H, R)`
3. Re-evaluate the specific question: what conditions are currently proven necessary or sufficient for a bootable 2D X-DOS clone.
4. Separate conclusions into three buckets:
   - confirmed
   - provisional
   - unknown
5. Make the note explicit about these sub-questions:
   - whether `boot-copy + file copy` is already proven sufficient
   - whether the first observed placement alone is enough to infer full runtime traversal
   - whether the current evidence proves or does not yet prove shared placement requirements
   - whether any write-side requirement can already be stated safely
6. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded bootable-clone reconciliation section.
7. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
8. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m15-xdos-bootable-clone-reconciliation`
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
- explicit note distinguishing confirmed versus merely plausible clone conditions
- explicit note confirming that unrelated local changes were not reset or cleaned

## Advancement Rule
- Do not start the next milestone from within this task
