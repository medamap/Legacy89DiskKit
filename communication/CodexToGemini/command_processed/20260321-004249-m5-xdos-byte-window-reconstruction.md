# Gemini Implementation Instruction

## Task ID
20260321-004249-m5-xdos-byte-window-reconstruction

## Objective
Advance the X-DOS kernel analysis workspace from label bootstrap to small, evidence-backed byte-window reconstruction. Focus on extracting and documenting real byte windows around filesystem-relevant syscall entrypoints and I/O-related logic, without attempting full-kernel disassembly.

## Branch
- Base: `develop`
- Name: `codex/m5-xdos-byte-window-reconstruction`
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Files To Read First
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/AGENTS.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/communication_rule.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/boot_and_io_notes.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/x1_io_ports_reference.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/x1_io_ports_reference.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88)

## Constraints
- Analysis-workspace changes only
- Do not edit production code
- Do not claim whole-function reconstruction unless the observed bytes actually support it
- Do not invent opcode bytes
- Use short byte windows with explicit source location
- If disassembly is uncertain, prefer raw `db` plus a note over speculative mnemonics
- Keep scope limited to filesystem-access relevant kernel windows

## Required Changes
1. Extend `analysis/xdos-kernel/read_path.asm`.
   - Add one or more small observed byte windows around the most useful known points.
   - Candidate focus areas:
     - the `EE 10` side-select area
     - one syscall entrypoint neighborhood if actual bytes can be located from a primary artifact
     - one FDC-related I/O pattern if actual bytes can be located
   - For every added byte window:
     - include the physical source location or address source
     - distinguish confirmed bytes from commentary
   - If a mnemonic is used, make sure the bytes are visible nearby or directly represented.

2. Update `analysis/xdos-kernel/labels.tsv`.
   - Add labels for any newly identified byte-window anchors or local branch targets
   - Keep evidence class conservative

3. Update `analysis/xdos-kernel/boot_and_io_notes.md`.
   - Add a short section:
     - "Observed Byte Windows"
   - For each new reconstructed window, record:
     - source disk
     - physical location
     - why it matters to read/write/boot analysis

4. Update `analysis/xdos-kernel/README.md` only if needed.
   - Only if you need one extra convention for byte-window sourcing

## Verification
- Show the final `read_path.asm` additions
- Ensure every new reconstructed region has an explicit source location
- Ensure no invented placeholder bytes were added
- Ensure only analysis workspace files are modified

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
- The analysis workspace moves from entrypoint inventory to actual observed byte-window reconstruction
- New assembly content is small, sourced, and conservative
- Future milestones can extend these windows rather than restarting from labels only
