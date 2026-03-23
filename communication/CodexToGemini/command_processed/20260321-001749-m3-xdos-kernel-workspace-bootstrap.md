# Gemini Implementation Instruction

## Task ID
20260321-001749-m3-xdos-kernel-workspace-bootstrap

## Objective
Create a repo-tracked X-DOS kernel analysis workspace and bootstrap a label-driven assembly reconstruction for the filesystem-access area only. This milestone is not about finishing the reverse engineering. It is about creating the analysis folder, recording trusted labels and entrypoints, and reconstructing only the minimum confirmed code/data needed for future read/write/boot analysis.

## Branch
- Base: `develop`
- Name: `codex/m3-xdos-kernel-workspace-bootstrap`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Files To Read First
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-000112-m1-xdos-kernel-reanalysis-inventory-report.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-000112-m1-xdos-kernel-reanalysis-inventory-report.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-001005-m2-xdos-read-path-analysis-report.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_completed/20260321-001005-m2-xdos-read-path-analysis-report.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_CSharp_Implementation_Spec.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88)

## Required Inputs
- Primary analysis target:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88`
- Secondary analysis target:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88`

## Constraints
- This milestone creates analysis assets, not production code changes
- Keep scope limited to filesystem-access relevant kernel areas
- Do not claim full-kernel disassembly
- Use conservative reconstruction:
  - bytes with confirmed control-flow meaning may be assembly instructions
  - bytes with uncertain role must remain `db`
  - data tables and code must not be mixed casually
- Every label must state its evidence class:
  - confirmed
  - probable
  - placeholder
- Do not edit existing Documents in this task
- Prefer ASCII-only analysis files unless raw byte or Japanese text evidence requires otherwise

## Required Changes
1. Create a dedicated analysis workspace in the repo.
   - Create:
     - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md`
     - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv`
     - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm`
     - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md`

2. Define the analysis conventions in `README.md`.
   - Explain:
     - source priority
     - evidence classes
     - label lifecycle
     - how code vs data is represented
     - which kernel areas are in scope for now

3. Seed `labels.tsv`.
   - Use a simple tab-separated schema:
     - address
     - label
     - class
     - source
     - note
   - Seed at least the known syscall and data addresses already grounded in repo materials:
     - `sys_file`
     - `sys_ropen`
     - `sys_rdd`
     - `sys_wopen`
     - `sys_wrd`
     - `sys_devi`
     - `sys_devo`
     - `sys_dtadr`
     - `sys_size`
     - `sys_exadr`

4. Create an initial `read_path.asm`.
   - Do not try to cover the whole kernel
   - Focus only on the minimum confirmed regions relevant to file reading
   - Use `org` and labels where justified
   - Use `db` for uncertain bytes or gaps
   - Include short comments only where they help distinguish:
     - confirmed instruction flow
     - probable flow
     - data region boundary
   - At minimum, capture:
     - known syscall entry labels
     - any directly evidenced code region around the interleaved-side logic (`EE 10`) if it is part of the read path evidence chain
     - any directly evidenced read-path-adjacent bytes that can be expressed without overclaiming

5. Create `boot_and_io_notes.md`.
   - This is not a broad design document
   - Keep it as a short analysis companion for:
     - fixed logical record constants already evidenced
     - device I/O call relationships
     - which parts are still unresolved and must remain data-first

## Verification
- Verify the new files exist and are readable
- Run a non-mutating listing of the new analysis directory
- If you extract bytes from disk images during the work, record the exact source offsets or logical locations in the report

## Deliverable
Write one Markdown report to:

- `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_waiting`

After completion:

- Move this instruction file to:
  - `/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/CodexToGemini/command_processed`

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

## Expected Result
- A durable repo-local workspace for X-DOS kernel reverse engineering
- Initial labels and assembly reconstruction that future milestones can extend safely
- Clear separation between confirmed code, probable code, and still-opaque bytes
