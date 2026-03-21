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

; --- Syscall Jump Table (Confirmed from XDOS_SYS.D88) ---
; Source: XDOS_SYS.D88 physical Track 6, R=1 (D88 offset 0x7c13)
; Pattern: 3-byte jump table entries ('jp addr').
; Memory range: 0xED78 to 0xEE00.

org 0xED78
sys_wopen:  db 0xC3, 0x76, 0xC8 ; Entry 0: Open file for write (jp 0xC876)
sys_wrd:    db 0xC3, 0x60, 0xC8 ; Entry 1: Write data from memory (jp 0xC860)
            db 0xC3, 0x66, 0xC8 ; Entry 2: [Unknown] (jp 0xC866)
sys_rdd:    db 0xC3, 0x6C, 0xC8 ; Entry 3: Read data into memory (jp 0xC86C)
sys_file:   db 0xC3, 0x98, 0xC8 ; Entry 4: Set active filename (jp 0xC898)
            db 0xC3, 0xA6, 0xC8 ; Entry 5: [Unknown] (jp 0xC8A6)
            db 0xC3, 0xB6, 0xC8 ; Entry 6: [Unknown] (jp 0xC8B6)
sys_devi:   db 0xC3, 0xC4, 0xC8 ; Entry 7: Device Input (jp 0xC8C4)
sys_devo:   db 0xC3, 0xD2, 0xC8 ; Entry 8: Device Output (jp 0xC8D2)
            db 0xC3, 0x1B, 0xC9 ; Entry 9: [Unknown] (jp 0xC91B)
sys_ropen:  db 0xC3, 0x14, 0xC9 ; Entry 10: Open file for read (jp 0xC914)
            ds 3 * (24 - 11)    ; Entries 11-23: [Unknown]
sys_load:   db 0xC3, 0xAA, 0xDE ; Entry 24: Load/Save (jp 0xDEAA)
            ds 3 * (40 - 25)    ; Entries 25-39: [Unknown]
sys_call:   db 0xC3, 0x1E, 0xCA ; Entry 40: Generic OS call dispatcher (jp 0xCA1E)
            ds 0xEE00 - $       ; Remainder of table area before fat_area

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
    db 0x38, 0xFB       ; jr c, -5 (Wait for Busy bit 0 to clear, targets: in a, (c))

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
fam_area:
    ; Sample bytes from XDOS_SYS.D88 FAM (Track 2, R=1)
    db 0x02, 0x02, 0x09, 0x03, 0x01, 0x0A, 0x04, 0x01
    db 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ds 512-16

org 0xEE00
fat_area:   ds 512     ; FAT bitmap area
