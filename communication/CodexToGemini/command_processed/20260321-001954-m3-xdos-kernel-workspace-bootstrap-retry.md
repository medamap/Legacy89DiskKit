# Gemini Implementation Instruction

## Task ID
20260321-001749-m3-xdos-kernel-workspace-bootstrap

## Objective
Retry the X-DOS kernel analysis workspace bootstrap. The workspace direction is acceptable, but the initial assembly reconstruction overreached by inserting placeholder bytes where no observed bytes were provided. Correct the workspace so that it can be safely grown over time without mixing confirmed bytes and invented filler.

## Branch
- Base: `develop`
- Name: `codex/m3-xdos-kernel-workspace-bootstrap`
- Continue on the same branch if possible
- Gemini may commit on this branch
- Gemini must not merge to `develop`

## Retry Reasons
- `read_path.asm` currently contains placeholder `db 0x00, 0x00, 0x00` blocks at syscall entrypoints without observed bytes
- That makes the file look more reconstructed than it really is
- The asm file should contain only:
  - confirmed bytes
  - confirmed mnemonics derived from those bytes
  - clearly bounded unknown regions without invented content

## Files To Read First
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/README.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/labels.tsv)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/analysis/xdos-kernel/read_path.asm)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-001749-m3-xdos-kernel-workspace-bootstrap.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/communication/GeminiToCodex/report_failed/20260321-001749-m3-xdos-kernel-workspace-bootstrap.md)
- [/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md](/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/Documents/X-DOS_Filesystem_Analysis.md)

## Constraints
- Keep the same workspace structure
- Do not broaden scope
- Do not invent opcode bytes
- If bytes are not observed, do not emit fake filler for them
- If a region is known only by entry address, represent it as a label plus a note, not fake instructions
- If a mnemonic is emitted, it must be backed by observed bytes

## Required Changes
1. Fix `analysis/xdos-kernel/read_path.asm`.
   - Remove invented placeholder bytes at syscall entrypoints
   - Replace them with one of:
     - label-only stubs with comments stating "entrypoint known, body not yet reconstructed"
     - actual observed bytes rendered as `db`
     - actual observed mnemonics rendered from confirmed bytes
   - For the `EE 10` area:
     - either express it as `db 0xEE, 0x10`
     - or as `xor 0x10`
     - but only if the surrounding context makes it clear that these are observed bytes from the cited location

2. Tighten `README.md` if needed.
   - Make explicit that unknown code bytes must not be synthesized

3. Keep `labels.tsv` and `boot_and_io_notes.md` only if they are still consistent after the asm correction.

## Verification
- Show the final contents of `analysis/xdos-kernel/read_path.asm`
- Ensure there are no fake placeholder byte runs such as `db 0x00, 0x00, 0x00` unless those bytes are directly observed and cited

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
- An analysis workspace that is conservative enough to extend safely
- No invented bytes in the bootstrap asm file
- A clearer distinction between known entrypoints and reconstructed code
