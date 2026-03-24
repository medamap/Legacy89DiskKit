# Gemini Implementation Instruction

## Task ID
20260324-034100-m30b-xdos-fam-directory-correlation-catalog

## Objective
Extend the raw evidence for FAM semantics by cataloging the nearest directly observed correlation candidates between directory-linked `0x1D/0x1E` pairs and sampled raw FAM windows, without upgrading any semantic grade.

## Branch
- Base: `develop`
- Name: `codex/m30b-xdos-fam-directory-correlation-catalog`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Required Inputs
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_fam.py
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/dump_dir_entries.py
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88

## Files To Read First
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/legacy89_context.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md
- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md

## Constraints
- Follow `communication/communication_rule.md`
- Follow `AGENTS.md`
- Use repository-local evidence only
- Mark uncertainty as `unknown`
- Do not upgrade any semantic grade in this task
- Do not claim that any specific FAM byte or nibble definitely equals `0x1D`, `0x1E`, track, sector, cluster, or allocation state
- Limit changes to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Steps
1. Use the existing helper scripts and accepted notes to inspect representative files where:
   - the `0x1D/0x1E` pair is known
   - the first observed placement pair is known
   - the raw FAM window is already cataloged
2. Identify only direct candidate correlations that can be stated conservatively, for example:
   - same file across disks shows same `0x1D/0x1E` and same raw FAM window
   - different files with different `0x1D/0x1E` show different raw FAM windows
   - repeated raw FAM windows occur despite differing directory-linked pairs
3. Add one new section to `boot_and_io_notes.md`:
   - `## Directory Pair vs FAM Correlation Candidates (Analysis-Only)`
   - with columns:
     - `sample case`
     - `observed directory-linked pair fact`
     - `observed raw FAM fact`
     - `current boundary`
4. Update `README.md` with one preserving-append sentence noting that this candidate-correlation section now exists.

## Verification
- `git diff -- /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md /Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`

## Deliverable
- Markdown report in `communication/GeminiToCodex/report_waiting/`

## Report Requirements
- branch_name
- summary
- changed_files
- commands
- evidence
- risks
- requested_review
- state explicitly whether the new section found only candidate correlations or any direct proven mapping

## User-Facing Handoff Block Rule
- If Codex also returns a copyable message for the user to forward to Gemini, do not nest code blocks inside that message
- Show commands as plain text list items instead
