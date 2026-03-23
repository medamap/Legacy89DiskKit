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

; --- Syscall Implementation Entrypoints (Confirmed from XDOS_SYS.D88) ---
; Source: XDOS_SYS.D88. Mapping: FileOffset = MemoryAddr - 0xED78 + 0x7c13.
; These are the targets of the syscall jump table.

org 0xC860
sys_wrd_impl:
    ; Offset: 0x56FB
    db 0xCD, 0x34, 0xC9 ; call 0xC934
    db 0xB7             ; or a
    db 0xCA, 0x38, 0xC9 ; jp z, 0xC938
    db 0xC9             ; ret

org 0xC86C
sys_rdd_impl:
    ; Offset: 0x5707
    db 0xFD, 0xB7, 0xC0 ; iy prefix (or dummy), or a, ret nz
    db 0xC3, 0xAF, 0xD6 ; jp 0xD6AF (Real implementation?)

org 0xC876
sys_wopen_impl:
    ; Offset: 0x5711
    db 0x17             ; rla
    db 0xCD, 0x34, 0xC9 ; call 0xC934
    db 0xFE, 0x13       ; cp 0x13
    db 0x20, 0x17       ; jr nz, +0x17
    db 0xCD, 0x34, 0xC9 ; call 0xC934
    db 0xB7             ; or a
    db 0x20, 0xFA       ; jr nz, -6
    db 0xCD, 0x7E, 0xC9 ; call 0xC97E

org 0xC898
sys_file_impl:
    ; Offset: 0x5733
    db 0xE3             ; ex (sp), hl
    db 0xC9             ; ret (returns to hl - likely skipping arguments)
    ; Note: Entry 4 is "Set active filename". This pattern suggests skipping inline data.

org 0xC8C4
sys_devi_impl:
    ; Offset: 0x575F
    db 0xCD, 0xBC, 0xC9 ; call 0xC9BC
    db 0xF6, 0x30       ; or 0x30
    db 0x1B             ; dec de
    db 0x12             ; ld (de), a
    db 0x10, 0xF7       ; djnz -9

org 0xC914
sys_ropen_impl:
    ; Offset: 0x57AF
    db 0x38, 0x07       ; jr c, +7
    db 0xFE, 0x11       ; cp 0x11
    db 0xD8             ; ret c
    db 0xD6, 0x07       ; sub 0x07
    db 0xFE, 0x10       ; cp 0x10
    db 0x3F             ; ccf
    db 0xC9             ; ret

; --- Helper Routines (Confirmed from XDOS_SYS.D88) ---
; Source: XDOS_SYS.D88. Mapping: FileOffset = MemoryAddr - 0x7165.

org 0xC934
helper_c934:
    ; Offset: 0x57CF
    db 0x02             ; ld (bc), a
    db 0x38, 0x0D       ; jr c, +0x0D
helper_c934_mid:
    db 0x0F, 0x0F, 0x0F, 0x0F ; rrca x4 (nibble swap)
    db 0x4F             ; ld c, a
    db 0x1A             ; ld a, (de)
    db 0x13             ; inc de
    db 0xCD, 0xEA, 0xC9 ; call 0xC9EA
    db 0x38, 0x01       ; jr c, +1
    db 0xB1             ; or c
    db 0xC1             ; pop bc
    db 0xC9             ; ret

org 0xC97E
helper_c97e:
    ; Offset: 0x5819
    db 0x78             ; ld a, b
    db 0xC1             ; pop bc
    db 0xB7             ; or a
    db 0xE1             ; pop hl
    db 0xC9             ; ret

org 0xC9BC
helper_c9bc:
    ; Offset: 0x5857
    db 0x3E, 0x50       ; ld a, 0x50
    db 0xCD, 0x32, 0xEB ; call 0xEB32
    db 0xE1             ; pop hl
    db 0xD1             ; pop de
    db 0xC1             ; pop bc
    db 0xC9             ; ret

org 0xD155
; Target window cataloged from helper_d6af sub-call
    db 0x04, 0x42, 0x0E, 0x00, 0xC9 ; literal: 0x00; transfer: ret

org 0xD1B5
; Target window cataloged from 0xD753 call
    db 0x01, 0x28, 0x03, 0x01, 0x22, 0x05, 0xC5, 0x21 ; confirmed: 0xD1B5 target window; literal: 0x0328, 0x0522

org 0xD6AF
helper_d6af:
    ; Offset: 0x654A
    db 0x1B, 0x1B       ; dec de, dec de
    db 0xCD, 0x55, 0xD1 ; call 0xD155
    db 0xCD, 0x0E, 0xE0 ; call 0xE00E
    db 0xD8             ; ret c
    db 0x3E, 0x08       ; ld a, 0x08
    db 0x37             ; scf
    db 0xC0             ; ret nz
    db 0x7E             ; ld a, (hl)
    db 0xFE, 0x80       ; cp 0x80
    db 0x3E, 0x08       ; ld a, 0x08
    db 0x37             ; scf
    db 0xC0             ; ret nz
    db 0x11, 0x1D, 0x00 ; ld de, 0x001D
    db 0x19             ; add hl, de
    db 0x56             ; ld d, (hl)
    db 0x23             ; inc hl
    db 0x5E             ; ld e, (hl)
    db 0xCD, 0xE8, 0xDE ; call 0xDEE8
    db 0xC3, 0x53, 0xD7 ; jp 0xD753 (Corrected jump target)

org 0xD753
; Target window cataloged from helper_d6af final jump
    db 0x40, 0x20, 0x0D, 0x13, 0xCD, 0xB5, 0xD1, 0x3E, 0x01 ; literals: 0x0D, 0xD1B5, 0x01; transfer: none

org 0xDEE8
; Target window cataloged from helper_d6af sub-call after 1D/1E load
    db 0x01, 0x40, 0x01, 0x11, 0xA8, 0x00, 0x21, 0x00, 0xEE, 0x19 ; ld bc, 0x0140, ld de, 0x00A8, ld hl, 0xEE00, add hl, de; literals: 0x0140, 0x00A8, 0xEE00; transfer: none; observation: address-load style

org 0xE00E
; Target window cataloged from helper_d6af sub-call
    db 0xEB, 0xDF, 0x38, 0x72, 0x06 ; literal: 0x72; transfer: jr c

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
