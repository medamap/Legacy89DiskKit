; X-DOS Kernel Read Path Reconstruction
; Target: Minimum confirmed code/data for read analysis

; --- I/O Port Equates (Probable/Hardware-known) ---
fdc_status_cmd: equ 0xFF8
fdc_track:      equ 0xFF9
fdc_sector:     equ 0xFFA
fdc_data:       equ 0xFFB
fdc_control:    equ 0xFFC   ; Drive/Side/Motor control latch
ipl_rom_on:     equ 0x1D00  ; Any output to 1DxxH enables IPL ROM
ipl_rom_off:    equ 0x1E00  ; Any output to 1ExxH disables IPL ROM

; --- Volume Record (Confirmed from Track 0, R=1 on XDOSUTIL.D88) ---
; Physical location: Track 0, Sector 1 (offset 0x10 from D88 track start)
volume_record:
    db 0x01         ; Record type: Volume
    db "X-DOS        Sys" ; Disk Label (16 bytes)
    db 0x00, 0x08, 0x00, 0xC0, 0x00, 0xC0 ; Reserved/Addresses
    db 0x88         ; Format type (X1 2D)
    db 0x24, 0x04, 0x17 ; BCD Date (84/04/17)
    db 0x05, 0x00, 0x08, 0x00 ; Reserved

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

; --- Interleaved Side-Select Logic (Confirmed from XDOSUTIL.D88) ---
; Found at XDOSUTIL.D88 physical Track 2, R=8 (D88 offset 0x4bd9)
; Logic: toggles side-select bit (bit 4) for FDC access (MB8877A style)

side_select_logic:
    db 0x21, 0x91, 0xE6 ; ld hl, 0xE691 (Probable side-select latch shadow)
    db 0x7E             ; ld a, (hl)
    db 0xEE, 0x10       ; xor 0x10 (toggle head bit 4)
    db 0x77             ; ld (hl), a
    db 0xE6, 0x10       ; and 0x10
    db 0x20, 0x02       ; jr nz, $+4
    db 0x14             ; inc d (dummy/branch filler?)
    db 0x37             ; scf
    db 0x0E, 0xFC       ; ld c, 0xFC
    db 0x7E             ; ld a, (hl)
    db 0xED, 0x79       ; out (c), a (Physical side select)

; --- FDC Port Access Pattern (Confirmed from XDOSUTIL.D88) ---
; Found at XDOSUTIL.D88 physical Track 2, R=8 (D88 offset 0x4b3c)
; Pattern: typical MB8877A status wait loop

fdc_wait_loop:
    db 0x01, 0xF8, 0x0F ; ld bc, 0x0FF8 (fdc_status_cmd)
    db 0xED, 0x78       ; in a, (c)
    db 0x0F             ; rrca
    db 0x38, 0xFB       ; jr c, fdc_wait_loop (Wait for Busy bit 0 to clear)

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
