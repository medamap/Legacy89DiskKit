# Gemini Implementation Instruction

## Task ID
20260321-160245-m14-xdos-geometry-translation-proof

## Objective
Prove the relationship between the observed placement pair `(C * 2 + H, R)` and the underlying D88 geometry model, including when and why the `C * 2 + H` transform is valid.

## Branch
- Base: `develop`
- Name: `codex/m14-xdos-geometry-translation-proof`
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
- Do not mention FAM, cluster, load address, or entry point
- Allowed wording:
  - "geometry translation"
  - "observed placement pair"
  - "D88 header tuple"
  - "exact transform"
  - "unknown"
- If you use or extend helper code, keep it under `analysis/xdos-kernel/`
- Do not leave new untracked helper files outside `analysis/xdos-kernel/`

## Steps
1. Create branch `codex/m14-xdos-geometry-translation-proof` from `develop`.
2. Use current tracked analysis assets to restate the independently observed placement method.
3. Prove, with tracked evidence, why `(C * 2 + H, R)` is the correct transform for the sampled 2D images.
4. Make the proof explicit about:
   - how `C`, `H`, and `R` are read from D88 headers
   - why `C * 2 + H` is used
   - whether this depends on side layout or 2D-specific assumptions
5. Update `analysis/xdos-kernel/boot_and_io_notes.md` with an evidence-graded geometry-translation section.
6. Update `analysis/xdos-kernel/README.md` only if the critical-unknown wording materially improves.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m14-xdos-geometry-translation-proof`
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
- explicit raw observation snippets or tracked helper output excerpts that justify the transform
- explicit note confirming that unrelated local changes were not reset or cleaned

## Advancement Rule
- Do not start the next milestone from within this task
