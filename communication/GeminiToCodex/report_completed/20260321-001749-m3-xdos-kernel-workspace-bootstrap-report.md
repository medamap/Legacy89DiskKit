# Gemini Task Report

- **Task ID**: 20260321-001749-m3-xdos-kernel-workspace-bootstrap
- **Instruction Filename**: 20260321-001954-m3-xdos-kernel-workspace-bootstrap-retry.md
- **Branch Name**: `codex/m3-xdos-kernel-workspace-bootstrap`

## Summary
Corrected the X-DOS kernel analysis workspace by removing invented placeholder bytes in `read_path.asm`. The workspace now strictly represents only confirmed bytes, mnemonics, or labeled entrypoints without synthesized filler. Updated `README.md` to explicitly forbid synthesizing unknown code bytes.

## Changed Files
- `analysis/xdos-kernel/read_path.asm`: Removed `db 0x00, 0x00, 0x00` and `ret` placeholders at syscall entrypoints; clarified `EE 10` representation.
- `analysis/xdos-kernel/README.md`: Added strict prohibition against synthesizing unknown code bytes.

## Commands
```bash
git add analysis/xdos-kernel/read_path.asm analysis/xdos-kernel/README.md
git commit -m "docs(analysis): fix xdos-kernel workspace bootstrap to remove invented filler bytes"
```

## Evidence
Final contents of `analysis/xdos-kernel/read_path.asm` showing no invented filler:
```asm
; X-DOS Kernel Read Path Reconstruction
; Target: Minimum confirmed code/data for read analysis

; --- Syscall Jump Table (Confirmed from x-dos.h) ---
; Note: Entrypoints are confirmed, bodies are not yet reconstructed.

org 0xED81
sys_rdd:
    ; Read data into memory using sys_dtadr
    ; [Entrypoint confirmed, body not yet reconstructed]

org 0xED84
sys_file:
    ; Set active filename (DE = pointer)
    ; [Entrypoint confirmed, body not yet reconstructed]

org 0xED8D
sys_devi:
    ; Device Input (HL=buf, DE=rec, A=cnt)
    ; [Entrypoint confirmed, body not yet reconstructed]

org 0xED90
sys_devo:
    ; Device Output (HL=buf, DE=rec, A=cnt)
    ; [Entrypoint confirmed, body not yet reconstructed]

org 0xED96
sys_ropen:
    ; Open file for read
    ; [Entrypoint confirmed, body not yet reconstructed]

; --- Interleaved Side-Select Logic (Confirmed from Salvaged Z80 Kernel) ---
; Found at physical C1, H1, R8 on XDOSUTIL.D88
; Logic: toggles side-select bit for FDC access (MB8877A style)

interleaved_side_select:
    db 0xEE, 0x10   ; xor 0x10 (toggle head bit for side selection)
    ; Note: Observed bytes EE 10 at this logic point.
    ; ... more logic likely follows to write to FDC control register
```

## Risks
- The assembly file `read_path.asm` may not be directly assemblable in its current state due to empty bodies or missing jumps, but it is now a faithful representation of confirmed knowledge.

## Requested Review
Verify that the removal of placeholders satisfies the "no invented bytes" requirement and that the `README.md` update provides sufficient guardrails for future expansion.
