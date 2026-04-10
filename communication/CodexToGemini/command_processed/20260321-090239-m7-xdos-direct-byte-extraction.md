# Gemini Implementation Instruction

## Task ID
20260321-090239-m7-xdos-direct-byte-extraction

## Objective
Use the real X-DOS D88 images to determine whether direct byte windows can be extracted for the filesystem-relevant syscall region around `0xED78`, and update the analysis workspace only with directly observed windows or clearly stated extraction limits.

## Branch
- Base: `develop`
- Name: `codex/m7-xdos-direct-byte-extraction`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/x1_io_ports_reference.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md`

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
- Do not rewrite unrelated communication history
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Prefer direct byte extraction from D88 artifacts over secondary documents
- Do not invent filler bytes, synthesized instructions, or guessed jump targets
- Do not widen scope into write-path reconstruction or boot-path reconstruction
- Limit updates to filesystem-relevant syscall region, extraction notes, and supporting labels only

## Steps
1. Create branch `codex/m7-xdos-direct-byte-extraction` from `develop`.
2. Determine how the current analysis mapped observed bytes from `XDOS_SYS.D88` and `XDOSUTIL.D88`, then reproduce that method using direct inspection tools or small local helper commands.
3. Attempt to extract direct byte windows that bear on the syscall region around:
   - `0xED78` (`sys_wopen`)
   - `0xED81` (`sys_rdd`)
   - `0xED84` (`sys_file`)
   - `0xED8D` (`sys_devi`)
   - `0xED96` (`sys_ropen`)
   - `0xEDC0` (`sys_load`)
   - `0xEDF0` (`sys_call`)
4. If direct windows are found, update `analysis/xdos-kernel/read_path.asm` with only those observed bytes and precise comments about their source image and physical location.
5. If direct windows cannot be found, do not invent structure. Instead:
   - add a short extraction-limit note to `analysis/xdos-kernel/boot_and_io_notes.md` or `README.md`
   - explain which mapping gap prevents direct observation
6. Update `analysis/xdos-kernel/labels.tsv` only if the direct extraction changes evidence class or provides a new directly supported label.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m7-xdos-direct-byte-extraction`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`
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
- explicit note saying whether any syscall-region bytes were directly observed from D88 images

## Advancement Rule
- Creating this instruction is allowed because the user explicitly asked to proceed to the next step
- Do not start the next milestone from within this task
