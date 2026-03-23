# Gemini Implementation Instruction

## Task ID
20260323-013728-m17d-xdos-fam-4bit-range-check

## Objective
Check whether the sampled and directly inspected FAM bytes stay within the `0x00..0x0F` range, and record that as a raw range observation without assigning semantic meaning.

## Task Kind
analysis-only

## Slice Rule
This task is only a range check. Do not interpret values as pointers, chains, flags, or counts. Only determine whether sampled FAM bytes and a wider inspected window stay within the low 4-bit range.

## Branch
- Base: `develop`
- Name: `codex/m17d-xdos-fam-4bit-range-check`
- Gemini may commit on this branch if the instruction requires implementation
- Gemini must not merge to `develop`

## Required Inputs
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_fam.py`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Files To Read First
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_fam.py`

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use evidence for every claim
- Mark uncertainty as `unknown`
- Do not add helper scripts unless strictly necessary
- Prefer existing tracked helpers
- Do not edit C# production code
- Do not resume implementation work
- Do not claim:
  - semantic meaning
  - chain meaning
  - allocation meaning
  - pointer meaning
  - count meaning
- Allowed conclusion terms:
  - `in-range`
  - `out-of-range`
  - `unknown`

## Steps
1. Use existing tracked helpers and/or direct disk observation to inspect the FAM sector (Track 2, Sector 1) for both sampled disks.
2. Confirm whether the sampled raw windows already recorded in `boot_and_io_notes.md` stay inside `0x00..0x0F`.
3. Extend the observation beyond the sampled windows just enough to inspect a wider raw span of the FAM sector on both disks.
4. Add a new section to `boot_and_io_notes.md` named exactly:
   - `## FAM 4-Bit Range Check (Analysis-Only)`
5. In that section, record:
   - inspected span
   - whether any byte exceeded `0x0F`
   - whether the observation is identical across the two sampled disks
6. Update `README.md` only if needed to note that the inspected FAM bytes remain within the low 4-bit range, while semantics remain unknown.

## Verification
- `python3 /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_fam.py`
- `git diff -- analysis/xdos-kernel/boot_and_io_notes.md analysis/xdos-kernel/README.md`

## Acceptance
- A new section with the exact heading exists
- The section states only range-check facts
- No semantic interpretation is introduced
- The diff is non-empty

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

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
