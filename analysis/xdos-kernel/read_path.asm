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

; --- File I/O Variables (Confirmed from x-dos.h) ---

org 0xECE2
sys_dtadr:  dw 0x0000  ; Word: Data load address
sys_size:   dw 0x0000  ; Word: Data size in bytes
sys_exadr:  dw 0x0000  ; Word: Execution address

; --- Buffer Areas (Confirmed from make_BGM) ---

org 0x7000
dir_area:   ds 512     ; Directory sector buffer
org 0x7200
bdir_area:  ds 512     ; bdir system code buffer
org 0x7400
fam_area:   ds 512     ; FAM cluster chain buffer
org 0xEE00
fat_area:   ds 512     ; FAT bitmap area
