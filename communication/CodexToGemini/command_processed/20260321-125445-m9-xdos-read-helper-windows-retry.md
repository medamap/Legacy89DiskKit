# Gemini Implementation Instruction

## Task ID
20260321-125154-m9-xdos-read-helper-windows

## Objective
Retry the X-DOS read-helper analysis conservatively, correcting the misread bytes in `helper_d6af` and keeping helper-role claims strictly tied to the directly observed byte windows.

## Branch
- Base: `develop`
- Name: `codex/m9-xdos-read-helper-windows-retry`
- Gemini may commit on this branch because tracked analysis assets will change
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-125154-m9-xdos-read-helper-windows-report.md`

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
- Use direct byte extraction from `XDOS_SYS.D88`
- Do not invent filler bytes, synthesized instructions, or guessed jump targets
- Keep helper-role descriptions conservative and byte-driven

## Why The Previous Attempt Failed
1. `helper_d6af` was misread at the final jump target.
2. The diff and report claimed `jp 0xD353`, but the actual bytes are `C3 53 D7` which means `jp 0xD753`.
3. Helper-role prose was slightly stronger than the direct bytes alone justify.

## Steps
1. Create branch `codex/m9-xdos-read-helper-windows-retry` from `develop`.
2. Re-extract the direct helper windows for:
   - `0xC934`
   - `0xC97E`
   - `0xC9BC`
   - `0xD6AF`
3. Correct `analysis/xdos-kernel/read_path.asm` so every byte sequence matches the actual image bytes.
4. Fix any note or label that depends on the mistaken `0xD353` reading.
5. Keep helper-purpose language conservative. For example:
   - "calls X and returns" is acceptable
   - "the heavy lifter for reads" is too strong unless the bytes alone directly support that conclusion
6. Update `analysis/xdos-kernel/boot_and_io_notes.md` and `analysis/xdos-kernel/labels.tsv` only as needed to match the corrected bytes and conservative interpretation.
7. Commit only the intended tracked analysis files.

## Verification
- Confirm the branch is `codex/m9-xdos-read-helper-windows-retry`
- `git diff --stat develop...HEAD`
- `git diff -- analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/labels.tsv analysis/xdos-kernel/boot_and_io_notes.md`
- `git status --short`
- Re-check `helper_d6af` with:
  - `hexdump -C -s 0x654a -n 48 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`

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
- explicit note confirming the corrected final jump target bytes for `helper_d6af`

## Advancement Rule
- This retry is allowed without new user approval because it is the same failed task
