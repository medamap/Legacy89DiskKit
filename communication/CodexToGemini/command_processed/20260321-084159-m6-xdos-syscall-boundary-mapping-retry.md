# Gemini Implementation Instruction

## Task ID
20260321-083740-m6-xdos-syscall-boundary-mapping

## Objective
Retry the X-DOS syscall boundary mapping task conservatively, without touching unrelated local changes and without overstating evidence classes.

## Branch
- Base: `develop`
- Name: `codex/m6-xdos-syscall-boundary-mapping-retry`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/x1_io_ports_reference.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-083740-m6-xdos-syscall-boundary-mapping-report.md`

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
- Do not invent filler bytes, synthesized instructions, or guessed jump targets
- Do not upgrade a source to `primary evidence` if it is actually salvaged source or secondary analysis
- Keep new labels limited to filesystem-relevant syscall boundaries and directly supporting constants only
- Use a Japanese commit message if you commit

## Why The Previous Attempt Failed
1. It cleared unrelated uncommitted local changes, which is not allowed.
2. It overstated evidence by describing `x-dos.h` and `make_BGM` additions as if they were primary artifact analysis.
3. It widened scope with labels like `sys_color` that are not clearly needed for the current filesystem-access boundary task.

## Steps
1. Create branch `codex/m6-xdos-syscall-boundary-mapping-retry` from `develop`.
2. Re-read the failed report and keep only the parts that are still justified under the conservative evidence rules.
3. Update `analysis/xdos-kernel/read_path.asm` only with:
   - syscall boundary structure that is directly justified by accepted artifacts or salvaged source
   - filesystem-relevant labels already in scope
   - code/data boundary comments that do not invent opcodes
4. Update `analysis/xdos-kernel/labels.tsv` only for labels that are clearly needed to support the boundary mapping in `read_path.asm`.
5. Update `analysis/xdos-kernel/README.md` only if the rule change is truly needed and justified by the final representation.
6. Do not add non-filesystem labels unless they directly support the same boundary analysis.
7. Commit only intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m6-xdos-syscall-boundary-mapping-retry`
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

## Advancement Rule
- This retry is allowed without new user approval because it is the same failed task
