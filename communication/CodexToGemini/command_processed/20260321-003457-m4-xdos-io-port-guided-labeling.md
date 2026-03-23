# Gemini Implementation Instruction

## Task ID
20260321-003457-m4-xdos-io-port-guided-labeling

## Objective
Extend the X-DOS kernel analysis workspace by using the newly added X1 I/O port reference to improve filesystem-access related labels and notes. Focus only on ports and labels that materially help analyze read/write/boot behavior, especially FDC, IPL ROM mapping, DMA, and memory/buffer handling. Do not broaden into unrelated graphics, sound, or UI hardware.

## Branch
- Base: `develop`
- Name: `codex/m4-xdos-io-port-guided-labeling`
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
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88)

## Constraints
- Analysis-workspace changes only
- Do not edit production code
- Do not broaden scope beyond filesystem-access relevant hardware usage
- Only add labels/notes when they are grounded in:
  - primary artifact bytes
  - salvaged source text
  - or careful, explicitly marked probable inference
- Do not invent opcode bytes
- Keep `read_path.asm` conservative

## Required Changes
1. Update `analysis/xdos-kernel/labels.tsv`.
   - Add filesystem-relevant hardware labels where justified, for example:
     - 5-inch FDC port group (`0x0FF8`-`0x0FFF`)
     - IPL ROM enable/disable ports (`1D**H`, `1E**H`) if they matter to boot analysis
     - DMA/SIO/CTC only if directly relevant to X-DOS disk I/O analysis
   - Use evidence classes carefully:
     - confirmed
     - probable
     - placeholder
   - If a port is added from the hardware reference only, do not mark it `confirmed` for X-DOS usage unless kernel evidence exists.

2. Update `analysis/xdos-kernel/boot_and_io_notes.md`.
   - Add a focused section:
     - "Filesystem-Relevant X1 Ports"
   - Include only ports that may affect:
     - disk access
     - boot ROM mapping
     - DMA-based transfer
     - memory/buffer switching relevant to kernel load/save
   - Distinguish:
     - hardware-known
     - X-DOS-usage-confirmed
     - X-DOS-usage-probable

3. Update `analysis/xdos-kernel/README.md` only if needed.
   - If you add a new evidence distinction such as "hardware-known but kernel-unconfirmed", document it briefly.

4. Update `analysis/xdos-kernel/read_path.asm` only if you can safely improve comments or labels without adding invented bytes.
   - This task does not require opcode expansion.
   - Comment-only clarification is acceptable if grounded.

## Verification
- Show the diff or final contents of any changed analysis files
- Ensure no production-code files are modified
- Ensure new labels include evidence classes and source notes

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
- The X-DOS analysis workspace captures hardware-side context that is directly useful for later boot/read/write reverse engineering
- Filesystem-relevant I/O ports are documented without overclaiming X-DOS usage
- The analysis workspace remains conservative and extendable
